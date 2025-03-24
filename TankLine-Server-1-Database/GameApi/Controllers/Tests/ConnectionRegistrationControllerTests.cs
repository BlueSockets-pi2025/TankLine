using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using GameApi.Controllers;
using GameApi.Data;
using GameApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class ConnectionRegistrationControllerTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly GameDbContext _context;
    private readonly ConnectionRegistrationController _controller;

    public ConnectionRegistrationControllerTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new GameDbContext(options);
        _mockConfiguration = new Mock<IConfiguration>();
        _controller = new ConnectionRegistrationController(_context, _mockConfiguration.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {

        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "", 
            Username = "user123",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);



    }

        [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameIsInvalid()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);
        
    }



    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordIsInvalid()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = "Password", // missing the special character as well as a numeric value 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Password must contain at least one numeric character and one special character.", badRequestResult.Value);
        
    }


        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordIsShort()
        {
            // Testing the case where the email is invalid
            var user = new UserAccount { 
                Email = "user@example.com", 
                Username = "user123",  
                PasswordHash = "P1!", // good format but password too short
                FirstName = "Firstname", 
                LastName = "Lastname"
            };

            // Act
            var result = await _controller.Register(user);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Password must be at least 8 characters long.", badRequestResult.Value);
            
        }



    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenFirstnameIsInvalid()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = "Password123!", 
            FirstName = "", 
            LastName = "Lastname"
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);
        
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenLastnameIsInvalid()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = ""
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);
        
    }


    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailIsWhitespaces()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "  ", 
            Username = "user123",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);

    }



    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameIsWhitespaces()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "  ",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);
        
    }




    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordIsWhitespaces()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = " ", // missing the special character as well as a numeric value 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);
        
    }



    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenFirstnameIsWhitespaces()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = "Password123!", 
            FirstName = "  ", 
            LastName = "Lastname"
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);
        
    }


    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenLastnameIsWhitespaces()
    {
        // Testing the case where the email is invalid
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "  "
        };

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The values cannot be empty or contain only whitespace.", badRequestResult.Value);
        
    }


//      PASSEDDDDDDD


    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
    {
        // Arrange
        var existingUser = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname",
            IsVerified = true 
        };        
        _context.UserAccounts.Add(existingUser);
        await _context.SaveChangesAsync();

        var user = new UserAccount{   
            Email = "otheruser@example.com", 
            Username = "user123",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };        
        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username or email already exists.", badRequestResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(existingUser);
        await _context.SaveChangesAsync();

    }


    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new UserAccount { 
            Email = "user@example.com", 
            Username = "my_user_1",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname", 
            IsVerified = true 
        };        
        _context.UserAccounts.Add(existingUser);
        await _context.SaveChangesAsync();

        // Assurez-vous de supprimer les utilisateurs avant de terminer le test (facultatif)
        var user = new UserAccount{   
            Email = "user@example.com", 
            Username = "my_user_2",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };        

        // Act
        var result = await _controller.Register(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username or email already exists.", badRequestResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(existingUser);
        await _context.SaveChangesAsync();
    }


    [Fact]
    public async Task Register_ShouldReturnOk_WhenUserIsSuccessfullyRegistered()
    {
        // Arrange
        var user = new UserAccount
        {   
            Email = "user@example.com", 
            Username = "my_user_2",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };     
        // Act
        var result = await _controller.Register(user);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Verification code sent to your email.", okResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();

    }




    [Fact]
    public async Task VerifyAccount_ShouldReturnBadRequest_WhenVerificationCodeIsExpired()
    {
        // Arrange
        var user = new UserAccount
        {   
            Email = "user@example.com", 
            Username = "my_user",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname", 
            IsVerified = false
        };     

        // force the modification of the information in the database (change the code and the expiration date to test)
        user.VerificationCode = "123456"; // force the code the to be 123456 in the db 
        user.VerificationExpiration = DateTime.UtcNow.AddMinutes(-5); // set the expiration date to two minutes 

        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new VerificationRequest { Email = "user@example.com", Code = "123456" };

        // Act
        var result = await _controller.VerifyAccount(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid or expired verification code.", badRequestResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();
    }



    [Fact]
    public async Task VerifyAccount_ShouldReturnBadRequest_WhenVerificationCodeIsWrong()
    {
        // Arrange
        var user = new UserAccount
        {   
            Email = "user@example.com", 
            Username = "my_user",  
            PasswordHash = "Password123!", 
            FirstName = "Firstname", 
            LastName = "Lastname"
        };     

        // force the modification of the information in the database (change the code to test)
        user.VerificationCode = "123456"; // force the code the to be 123456 in the db 

        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new VerificationRequest { Email = "user@example.com", Code = "111111" }; // wrong verification code

        // Act
        var result = await _controller.VerifyAccount(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid or expired verification code.", badRequestResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();
    }


    [Fact]
    public async Task ResendVerificationCode_ShouldReturnBadRequest_WhenAccountAlreadyVerified()
    {
        // Arrange
            var user = new UserAccount { 
                Email = "user@example.com", 
                Username = "user123",  
                PasswordHash = "hashedPassword123",  
                FirstName = "Firstname", 
                LastName = "Lastname", 
                IsVerified = true
            };
            
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new ResendVerificationRequest { Email = "user@example.com" };

        // Act
        var result = await _controller.ResendVerificationCode(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Account already verified.", badRequestResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();
    }

//      -- PASSEDD 


    [Fact]
    public async Task ResendVerificationCode_ShouldReturnBadRequest_WhenUserDoesNotExist()
    {
        var request = new ResendVerificationRequest { Email = "user@example.com" };

        // Act
        var result = await _controller.ResendVerificationCode(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task ResendVerificationCode_ShouldReturnOk_WhenVerificationCodeIsResent()
    {
        // Arrange
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = "Password123!",  
            IsVerified = false ,
            FirstName = "Firstname", 
            LastName = "Lastname"
        };

        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new ResendVerificationRequest { Email = "user@example.com" };

        // Act
        var result = await _controller.ResendVerificationCode(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("New verification code sent to your email.", okResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();

    }

    //--------------------------------------- PASSWORD RESET 

    [Fact]
    public async Task RequestPasswordReset_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var request = new PasswordResetRequest { Email = "nonexistent@example.com" };

        // Act
        var result = await _controller.RequestPasswordReset(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found.", badRequestResult.Value);
    }




    [Fact]
    public async Task RequestPasswordReset_ShouldReturnOk_WhenUserFoundAndResetCodeSent()
    {
        // Arrange
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123",  
            PasswordHash = "Password123!",  
            IsVerified = false ,
            FirstName = "Firstname", 
            LastName = "Lastname"
        };

        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new PasswordResetRequest { Email = "user@example.com" };

        // Act
        var result = await _controller.RequestPasswordReset(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Password reset code has been sent to your email.", okResult.Value);
        
         // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();
    }


    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenUserNotFound()
    {

        var request = new ResetPasswordRequest 
        { 
            Email = "nonexistent@example.com", 
            Code = "123456", 
            NewPassword = "NewPassword123!", 
            ConfirmPassword = "NewPassword123!"
         };

        // Act
        var result = await _controller.ResetPassword(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenResetCodeIsExpired()
    {
        // Arrange
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123", 
            PasswordResetToken = "123456", 
            PasswordHash = "Password123!" , 
            FirstName = "Firstname", 
            LastName = "Lastname", 
            PasswordResetExpiration = DateTime.UtcNow.AddMinutes(-1) // Expired token
        };
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new ResetPasswordRequest 
        { 
            Email = "user@example.com", 
            Code = "123456",
            NewPassword = "NewPassword123!",
            ConfirmPassword ="NewPassword123!"
        };

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Reset code expired.", badRequestResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenCodeDoesNotMatch()
    {
        // Arrange
        var user = new UserAccount { 
            Email = "user@example.com", 
            Username = "user123", 
            PasswordResetToken = "123456", 
            PasswordResetExpiration = DateTime.UtcNow.AddMinutes(5),
            FirstName = "Firstname", 
            LastName = "Lastname", 
            PasswordHash="Password123!"
        };
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new ResetPasswordRequest
        
        { 
            Email = "user@example.com", 
        Code = "654321", 
        NewPassword = "NewPassword123!" , 
        ConfirmPassword = "NewPassword123!"

        };

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid reset code.", badRequestResult.Value);

         // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnOk_WhenPasswordResetSuccessfully()
    {
        // Arrange
        var user = new UserAccount { 
                   
            Email = "user@example.com", 
            Username = "user123", 
            PasswordResetToken = "123456", 
            PasswordResetExpiration = DateTime.UtcNow.AddMinutes(5),
            FirstName = "Firstname", 
            LastName = "Lastname", 
            PasswordHash="Password123!", 
            };
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new ResetPasswordRequest 
        { Email = "user@example.com", Code = "123456", NewPassword = "NewPassword123!",         ConfirmPassword = "NewPassword123!" };

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Password reset successfully.", okResult.Value);
        
        
         // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenPasswordIsNotValid()
    {
        // Arrange
        var user = new UserAccount { 
                   
            Email = "user@example.com", 
            Username = "user123", 
            PasswordResetToken = "123456", 
            PasswordResetExpiration = DateTime.UtcNow.AddMinutes(5),
            FirstName = "Firstname", 
            LastName = "Lastname", 
            PasswordHash="Password123!", 
            };

        
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new ResetPasswordRequest
        { Email = "user@example.com", Code = "123456", NewPassword = "badPassword",    ConfirmPassword = "badPassword" };

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.", badRequestResult.Value);

        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user) ; 
        await _context.SaveChangesAsync();
    }


    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var user = new UserAccount { 
                   
            Email = "user@example.com", 
            Username = "user123", 
            PasswordResetToken = "123456", 
            PasswordResetExpiration = DateTime.UtcNow.AddMinutes(5),
            FirstName = "Firstname", 
            LastName = "Lastname", 
            PasswordHash="Password123!", 
            };

        
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        var request = new ResetPasswordRequest
        { Email = "user@example.com", Code = "123456", NewPassword = "Password123!",    ConfirmPassword = "NotmatchingPassword123!" };

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Passwords do not match", badRequestResult.Value);

        
        // Cleanup: Remove the added user after the test 
        _context.UserAccounts.Remove(user) ; 
        await _context.SaveChangesAsync();
    }


}
