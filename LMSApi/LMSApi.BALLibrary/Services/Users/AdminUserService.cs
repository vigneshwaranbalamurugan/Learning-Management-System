using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LMSApi.BALLibrary.Interfaces.Users;
using LMSApi.BALLibrary.Utils;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs.UserManagement;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services.Users
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(IUserRepository userRepository, IMapper mapper, ILogger<AdminUserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedUserListResponse> GetUsersPagedAsync(UserSearchQuery query)
        {
            var (users, totalCount) = await _userRepository.GetAllUsersPagedAsync(query);

            var userResponses = _mapper.Map<IEnumerable<AdminUserResponse>>(users);
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            return new PagedUserListResponse
            {
                Users = userResponses,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<AdminUserResponse> CreateUserAsync(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (await _userRepository.IsEmailAlreadyRegisteredAsync(request.Email))
            {
                throw new InvalidOperationException($"Email {request.Email} is already registered.");
            }

            var (passwordHash, passwordSalt) = PasswordHashing.HashPassword(request.Password);

            // Determine RoleId based on request.Role
            var roleId = request.Role.ToLower() switch
            {
                "learner" => 1,
                "instructor" => 2,
                "admin" => 3,
                _ => throw new ArgumentException("Invalid role specified. Must be Learner, Instructor, or Admin.")
            };

            var user = new LMSApi.ModelLibrary.Models.Users
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsActive = true,
                IsEmailVerified = true, // Created by admin, auto-verify
                RoleId = roleId,
                CurrentTokenType = TokenType.EmailVerification,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            
            // Re-fetch to include the Role object for mapping
            var fetchedUser = await _userRepository.GetByEmailAsync(user.Email);

            if (fetchedUser == null)
            {
                throw new InvalidOperationException("User creation failed.");
            }

            return _mapper.Map<AdminUserResponse>(fetchedUser);
        }
    }
}
