using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.Settings;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;

namespace LMSApi.BALLibrary.Services
{
    /// <summary>
    /// Razorpay payment provider using:
    /// - Standard Orders API for order creation
    /// - Route API (POST /payments/{id}/transfers) for instructor split payouts
    /// - Test mode compatible
    /// </summary>
    public class RazorpayPaymentProvider : IPaymentProvider
    {
        private readonly RazorpaySettings _settings;
        private readonly string _webhookSecret;
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public RazorpayPaymentProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _settings = new RazorpaySettings
            {
                KeyId = configuration["Razorpay:KeyId"] ?? string.Empty,
                KeySecret = configuration["Razorpay:KeySecret"] ?? string.Empty
            };
            _webhookSecret = configuration["Razorpay:WebhookSecret"] ?? string.Empty;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient("RazorpayRoute");
        }

        public string ProviderName => "Razorpay";

        // ── Standard Payment Order ─────────────────────────────────────────────
        public Task<string> CreateOrderAsync(decimal amount, string currency, string receiptId)
        {
            int amountInPaisa = (int)(amount * 100);
            Dictionary<string, object> options = new()
            {
                { "amount", amountInPaisa },
                { "currency", currency },
                { "receipt", receiptId }
            };

            RazorpayClient client = new(_settings.KeyId, _settings.KeySecret);
            Order order = client.Order.Create(options);
            return Task.FromResult(order["id"].ToString());
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            Dictionary<string, string> attributes = new()
            {
                { "razorpay_order_id", orderId },
                { "razorpay_payment_id", paymentId },
                { "razorpay_signature", signature }
            };
            try
            {
                Razorpay.Api.Utils.verifyPaymentSignature(attributes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── Razorpay Route API ─────────────────────────────────────────────────
        // Route allows splitting a captured payment to a Linked Account (instructor).
        // In test mode: create linked accounts in Razorpay dashboard → get acc_xxx ID.
        // Instructor stores their acc_xxx in InstructorPayoutAccount.RazorpayFundAccountId.

        /// <summary>
        /// Creates a Route transfer from a captured payment to a linked account.
        /// Endpoint: POST /v1/payments/{payment_id}/transfers
        /// </summary>
        public async Task<string> CreateRouteTransferAsync(
            string paymentId, string linkedAccountId, decimal amount, string currency)
        {
            int amountInPaisa = (int)(amount * 100);

            var payload = new
            {
                transfers = new[]
                {
                    new
                    {
                        account = linkedAccountId,
                        amount = amountInPaisa,
                        currency,
                        on_hold = false  // release immediately in test mode
                    }
                }
            };

            var response = await PostRazorpayAsync($"payments/{paymentId}/transfers", payload);

            // Response is { "entity": "collection", "items": [{ "id": "trf_xxx", ... }] }
            var items = response.GetProperty("items");
            if (items.GetArrayLength() == 0)
                throw new Exception("Razorpay Route transfer returned no items.");

            return items[0].GetProperty("id").GetString()!;
        }

        /// <summary>Placeholder — Contact management is for Payouts X (not needed for Route).</summary>
        public Task<string> CreateOrGetContactAsync(string name, string email, string contactType = "vendor")
        {
            // Route does not use Contacts. Linked accounts are created in Razorpay Dashboard.
            throw new NotSupportedException("Use Razorpay Route with linked accounts (acc_xxx). Contacts are only for Payouts X.");
        }

        /// <summary>
        /// For Route: stores the instructor's Razorpay Linked Account ID (acc_xxx).
        /// This is NOT a fund account — it's obtained from the Razorpay dashboard.
        /// </summary>
        public Task<string> CreateFundAccountAsync(
            string contactId, string accountHolderName, string accountNumber, string ifscCode)
        {
            throw new NotSupportedException(
                "For Razorpay Route: instructors submit their Linked Account ID (acc_xxx) directly. " +
                "No fund account creation needed.");
        }

        /// <summary>Routes a payment to instructor's linked account using the Route API.</summary>
        public async Task<string> CreatePayoutAsync(
            string linkedAccountId, decimal amount, string currency, string paymentId, string? narration = null)
        {
            // For Route, paymentId is the Razorpay payment ID (pay_xxx)
            return await CreateRouteTransferAsync(paymentId, linkedAccountId, amount, currency);
        }

        public bool VerifyWebhookSignature(string payload, string signature, string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            var data = Encoding.UTF8.GetBytes(payload);
            using var hmac = new HMACSHA256(key);
            var computed = BitConverter.ToString(hmac.ComputeHash(data))
                .Replace("-", "").ToLowerInvariant();
            return computed == signature;
        }

        public async Task<LinkedAccountResult> CreateLinkedAccountAsync(
            string email,
            string phone,
            string legalBusinessName,
            string contactName,
            string businessType,
            string profileCategory,
            string profileSubcategory,
            string street1,
            string? street2,
            string city,
            string state,
            string postalCode,
            string country,
            string pan,
            string? gst,
            string accountNumber,
            string ifscCode)
        {
            var client = _httpClientFactory.CreateClient("RazorpayClient");

            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}")
            );

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var payload = new
            {
                email = email,
                phone = phone,
                type = "route",
                legal_business_name = legalBusinessName,
                business_type = businessType,
                contact_name = contactName,
                profile = new 
                { 
                    category = profileCategory,
                    subcategory = profileSubcategory,
                    addresses = new
                    {
                        registered = new
                        {
                            street1 = street1,
                            street2 = street2,
                            city = city,
                            state = state,
                            postal_code = postalCode,
                            country = country
                        }
                    }
                },
                legal_info = new 
                { 
                    pan = pan,
                    gst = gst
                }
            };

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var response = await client.PostAsJsonAsync(
                "https://api.razorpay.com/v2/accounts",
                payload,
                options
            );

            

            var resultString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Razorpay Create Account API error: {resultString}");
            }
        
            var resultJson = JsonDocument.Parse(resultString);
            var accountId = resultJson.RootElement.GetProperty("id").GetString()!;

            // 2. Create Stakeholder
            var stakeholderId = await CreateStakeholderAsync(accountId, contactName, email);

            // 3. Create Product Configuration
            var productId = await CreateProductConfigurationAsync(accountId, "route");

            // 4. Update Product Configuration with bank details (which implicitly requests activation when tnc_accepted = true)
            await UpdateProductConfigurationAsync(accountId, productId, accountNumber, ifscCode, contactName);

            return new LinkedAccountResult
            {
                AccountId = accountId,
                StakeholderId = stakeholderId,
                ProductId = productId
            };
        }

        public async Task UpdateLinkedAccountAsync(
            string accountId,
            string? stakeholderId,
            string? productId,
            string email,
            string phone,
            string legalBusinessName,
            string contactName,
            string profileCategory,
            string profileSubcategory,
            string street1,
            string? street2,
            string city,
            string state,
            string postalCode,
            string country,
            string pan,
            string? gst,
            string accountNumber,
            string ifscCode)
        {
            var client = _httpClientFactory.CreateClient("RazorpayClient");

            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}")
            );

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var payload = new
            {
                phone = phone,
                legal_business_name = legalBusinessName,
                contact_name = contactName,
                profile = new 
                { 
                    category = profileCategory,
                    subcategory = profileSubcategory,
                    addresses = new
                    {
                        registered = new
                        {
                            street1 = street1,
                            street2 = street2,
                            city = city,
                            state = state,
                            postal_code = postalCode,
                            country = country
                        }
                    }
                },
                legal_info = new 
                { 
                    pan = pan,
                    gst = gst
                }
            };

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Patch, $"https://api.razorpay.com/v2/accounts/{accountId}")
            {
                Content = JsonContent.Create(payload, options: options)
            };

            var response = await client.SendAsync(request);
            
            var resultString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Razorpay Update Account API error: {resultString}");
            }

            // Update Stakeholder if exists
            if (!string.IsNullOrEmpty(stakeholderId))
            {
                await UpdateStakeholderAsync(accountId, stakeholderId, contactName, email);
            }

            // Update Product Configuration if exists
            if (!string.IsNullOrEmpty(productId))
            {
                await UpdateProductConfigurationAsync(accountId, productId, accountNumber, ifscCode, contactName);
            }
        }

        public async Task<string> CreateLinkedAccountOnlyAsync(
            string email,
            string phone,
            string legalBusinessName,
            string contactName,
            string businessType,
            string profileCategory,
            string profileSubcategory,
            string street1,
            string? street2,
            string city,
            string state,
            string postalCode,
            string country,
            string pan,
            string? gst,
            string referenceId)
        {
            var client = _httpClientFactory.CreateClient("RazorpayClient");

            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}")
            );

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var payload = new
            {
                email = email,
                phone = phone,
                type = "route",
                legal_business_name = legalBusinessName,
                business_type = businessType,
                contact_name = contactName,
                profile = new 
                { 
                    category = profileCategory,
                    subcategory = profileSubcategory,
                    addresses = new
                    {
                        registered = new
                        {
                            street1 = street1,
                            street2 = street2,
                            city = city,
                            state = state,
                            postal_code = postalCode,
                            country = country
                        }
                    }
                },
                legal_info = new 
                { 
                    pan = pan,
                    gst = gst
                }
            };

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var response = await client.PostAsJsonAsync(
                "https://api.razorpay.com/v2/accounts",
                payload,
                options
            );

            var resultString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Razorpay Create Account API error: {resultString}");
            }
        
            var resultJson = JsonDocument.Parse(resultString);
            return resultJson.RootElement.GetProperty("id").GetString()!;
        }

        public async Task UpdateLinkedAccountOnlyAsync(
            string accountId,
            string email,
            string phone,
            string legalBusinessName,
            string contactName,
            string profileCategory,
            string profileSubcategory,
            string street1,
            string? street2,
            string city,
            string state,
            string postalCode,
            string country,
            string pan,
            string? gst)
        {
            var client = _httpClientFactory.CreateClient("RazorpayClient");

            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}")
            );

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var payload = new
            {
                email = email,
                phone = phone,
                legal_business_name = legalBusinessName,
                contact_name = contactName,
                profile = new 
                { 
                    category = profileCategory,
                    subcategory = profileSubcategory,
                    addresses = new
                    {
                        registered = new
                        {
                            street1 = street1,
                            street2 = street2,
                            city = city,
                            state = state,
                            postal_code = postalCode,
                            country = country
                        }
                    }
                },
                legal_info = new 
                { 
                    pan = pan,
                    gst = gst
                }
            };

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Patch, $"https://api.razorpay.com/v2/accounts/{accountId}")
            {
                Content = JsonContent.Create(payload, options: options)
            };

            var response = await client.SendAsync(request);
            
            var resultString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Razorpay Update Account API error: {resultString}");
            }
        }

        public async Task<string> CreateStakeholderOnlyAsync(string accountId, string name, string email)
        {
            return await CreateStakeholderAsync(accountId, name, email);
        }

        public async Task UpdateStakeholderOnlyAsync(string accountId, string stakeholderId, string name, string email)
        {
            await UpdateStakeholderAsync(accountId, stakeholderId, name, email);
        }

        public async Task<string> CreateProductConfigurationOnlyAsync(string accountId, string productName)
        {
            return await CreateProductConfigurationAsync(accountId, productName);
        }

        public async Task UpdateProductConfigurationOnlyAsync(string accountId, string productId, string accountNumber, string ifscCode, string beneficiaryName)
        {
            await UpdateProductConfigurationAsync(accountId, productId, accountNumber, ifscCode, beneficiaryName);
        }


        private async Task<string> CreateStakeholderAsync(string accountId, string name, string email)
        {
            var payload = new
            {
                name = name,
                email = email,
                relationship = new
                {
                    director = true,
                    executive = true
                }
            };
            var response = await PostRazorpayV2Async($"/v2/accounts/{accountId}/stakeholders", payload);
            return response.GetProperty("id").GetString()!;
        }

        private async Task UpdateStakeholderAsync(string accountId, string stakeholderId, string name, string email)
        {
            var client = _httpClientFactory.CreateClient("RazorpayClient");
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var payload = new
            {
                name = name,
                email = email
            };

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Patch, $"https://api.razorpay.com/v2/accounts/{accountId}/stakeholders/{stakeholderId}")
            {
                Content = JsonContent.Create(payload)
            };

            var response = await client.SendAsync(request);
            var resultString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Razorpay Update Stakeholder API error: {resultString}");
            }
        }

        private async Task<string> CreateProductConfigurationAsync(string accountId, string productName)
        {
            var payload = new
            {
                product_name = productName
            };
            var response = await PostRazorpayV2Async($"/v2/accounts/{accountId}/products", payload);
            return response.GetProperty("id").GetString()!;
        }

        private async Task UpdateProductConfigurationAsync(string accountId, string productId, string accountNumber, string ifscCode, string beneficiaryName)
        {
            var client = _httpClientFactory.CreateClient("RazorpayClient");
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var payload = new
            {
                tnc_accepted = true,
                settlements = new
                {
                    account_number = accountNumber,
                    ifsc_code = ifscCode,
                    beneficiary_name = beneficiaryName
                }
            };

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Patch, $"https://api.razorpay.com/v2/accounts/{accountId}/products/{productId}")
            {
                Content = JsonContent.Create(payload, options: options)
            };

            var response = await client.SendAsync(request);
            var resultString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Razorpay Update Product Config error: {resultString}");
            }
        }



        // ── Helper ─────────────────────────────────────────────────────────────
        private async Task<JsonElement> PostRazorpayV2Async(string endpoint, object payload)
        {
            var client = _httpClientFactory.CreateClient("RazorpayClient");
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var response = await client.PostAsJsonAsync($"https://api.razorpay.com{endpoint}", payload);
            var resultString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Razorpay V2 API error on {endpoint}: {resultString}");
            }

            return JsonDocument.Parse(resultString).RootElement;
        }

        private async Task<JsonElement> PostRazorpayAsync(string endpoint, object payload)
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}"));

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post,
                $"https://api.razorpay.com/v1/{endpoint}");
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.SendAsync(request);
            var content = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception($"Razorpay Route API error [{endpoint}]: {content}");

            return JsonDocument.Parse(content).RootElement;
        }
    }
}
