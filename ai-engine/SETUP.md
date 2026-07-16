# 1. Set up the Python engine
cd ai-engine && cp .env.template .env  # fill in GROQ_API_KEY + INTERNAL_API_KEY

python3.12 -m venv venv

source venv/bin/activate

pip install -r requirements.txt

# 2. Apply the DB migration
cd LMSApi && dotnet ef database update --project LMSApi.DALLibrary --startup-project LMSApi.API

# 3. Run the AI engine
uvicorn main:app --host 0.0.0.0 --port 8001

# 4. Add to appsettings.json:
#    "AiEngine": { "BaseUrl": "http://localhost:8001", "InternalApiKey": "<same key as .env>" }
 
