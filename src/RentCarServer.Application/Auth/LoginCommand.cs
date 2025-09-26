using FluentValidation;
using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.LoginTokens.ValueObjects;
using RentCarServer.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Auth;

public sealed record LoginCommand(
    string UserNameorEmail,
    string Password) : IRequest<Result<LoginCommandResponse>>;

public sealed class LoginCommandResponse
{
    public string? Token { get; set; }
    public string? TFACode { get; set; }
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(p => p.UserNameorEmail)
            .NotEmpty().WithMessage("Kullanıcı adı ya da email boş olamaz.")
            .MaximumLength(100).WithMessage("Kullanıcı adı ya da email en fazla 100 karakter olabilir.");
        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            //.MinimumLength(6).WithMessage("Şifre en az 6 karakter olabilir.")
            .MaximumLength(50).WithMessage("Şifre en fazla 50 karakter olabilir.");
    }
}

public sealed class LoginCommandHandler(
    IUserRepository userRepository, 
    IMailService mailService,
    IUnitOfWork unitOfWork,
    IJwtProvider jwtProvider) : 
    IRequestHandler<LoginCommand, Result<LoginCommandResponse>>
{
    public async Task<Result<LoginCommandResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p =>
        p.Email.Value == request.UserNameorEmail
        || p.UserName.Value == request.UserNameorEmail);

        if (user is null)
        {
            return Result<LoginCommandResponse>.Failure("Kullanıcı adı ya da şifre yanlış.");
        }

        var checkPassword = user.VerifyPasswordHash(request.Password);
        if (!checkPassword)
        {
            return Result<LoginCommandResponse>.Failure("Kullanıcı adı ya da şifre yanlış.");
        }

        if (!user.TFAStatus.Value)
        {
            var token = await jwtProvider.CreateTokenAsync(user, cancellationToken);

            var res = new LoginCommandResponse() { Token = token };
            return res;
        }
        else
        {
            user.CreateTFACode();
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            string to = user.Email.Value;
            string subject = "RentCarServer İki Adımlı Doğrulama Kodu";
            string body = @$"Uygulamaya girmek için aşağıdaki kodu kullanabilirsiniz:
                            <br/>
                            <h2>{user.TFAConfirmCode!.Value}</h2>";
            await mailService.SendAsync(to, subject, body, cancellationToken);

            var res = new LoginCommandResponse() { TFACode = user.TFACode!.Value };

            return res;
        }

    }
}