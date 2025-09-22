using FluentValidation;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Auth;

public sealed record LoginCommand(
    string UserNameorEmail,
    string Password) : IRequest<Result<string>>;

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
    IJwtProvider jwtProvider) : 
    IRequestHandler<LoginCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p =>
        p.Email.Value == request.UserNameorEmail
        || p.UserName.Value == request.UserNameorEmail);

        if (user is null)
        {
            return Result<string>.Failure("Kullanıcı adı ya da şifre yanlış.");
        }

        var checkPassword = user.VerifyPasswordHash(request.Password);
        if (!checkPassword)
        {
            return Result<string>.Failure("Kullanıcı adı ya da şifre yanlış.");
        }

        var token = await jwtProvider.CreateTokenAsync(user, cancellationToken);

        return token;
    }
}