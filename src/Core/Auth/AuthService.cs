using System.Data.Common;
using BotSaaS.Api.Core.Companies;
using BotSaaS.Api.Core.Users;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;
using BotSaaS.Api.Shared.Security;

namespace BotSaaS.Api.Core.Auth;

// Auth: registration (Company + User in one transaction) + login (verify password + issue JWT).
public class AuthService : IAuthService
{
    private readonly IDatabase _databaseConnection;
    private readonly DbSession _dbSession;
    private readonly IUserService _userService; 
    private readonly ICompaniesService _companiesService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IDatabase databaseConnection, DbSession dbSession, IUserService userService, ICompaniesService companiesService, IJwtTokenGenerator tokenGenerator, IPasswordHasher passwordHasher)
    {
        _databaseConnection = databaseConnection;
        _dbSession = dbSession;
        _userService = userService;
        _companiesService = companiesService;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<Result<User>> Register(RegisterRequest request)
    {
        // Open one connection+transaction and store them in the session, so the repos below run inside THIS same transaction.

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;

        await connection.OpenAsync();

        using DbTransaction transaction = await connection.BeginTransactionAsync();
        _dbSession.Transaction = transaction;

        try
        {
            Company company = await _companiesService.CreateCompany(request.CompanyName, request.Segment);
            User user = await _userService.CreateUser(request.OwnerName, request.Email, request.Password, company.Id);
           
            await transaction.CommitAsync();

            return Result<User>.Success(user); // return sucess with entity user

        } catch (DbException)
        {
            // any DB failure - undo everything (no orphan Company)
            await transaction.RollbackAsync();
            return Result<User>.Failure("Erro ao registrar");
        }
    }
    // Login: find user by email, verify password, return a JWT. Read-only, no transaction.
    public async Task<Result<string>> Login(LoginRequest request)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        // same message for missing email AND wrong password -- don't reveal which (security)
        User? user = await _userService.GetUserByEmail(request.Email);

        if(user is null) return Result<string>.Failure("Credenciais inválidas");

        bool userValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if(!userValid) return Result<string>.Failure("Credenciais inválidas");

        string token = _tokenGenerator.GenerateToken(user.Id, user.CompanyId, user.Email);
        return Result<string>.Success(token);
    }
}