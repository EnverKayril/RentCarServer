using GenericRepository;
using RentCarServer.Domain.Users;
using RentCarServer.Domain.Users.ValueObjects;

namespace RentCarServer.WebAPI;

public static class ExtensionMethods
{
    public static async Task CreateFirstUser(this WebApplication app)
    {
        using var scoped = app.Services.CreateScope();
        var userRepository = scoped.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scoped.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if(!(await userRepository.AnyAsync(p => p.UserName.Value == "admin")))
        {
            FirstName firstName = new("Enver");
            LastName lastName = new("Kayrıl");
            Email email = new("enverkayril@gmail.com");
            UserName userName = new("admin");
            Password password = new("1");


            var user = new User(firstName, lastName, email, userName, password);

            userRepository.Add(user);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
