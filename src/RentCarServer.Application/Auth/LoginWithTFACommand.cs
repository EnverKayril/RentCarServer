using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Auth;

public sealed record LoginWithTFACommand(
    string EmailOrUserName,
    string TFACode,
    string TFAConfirmCade) : IRequest<Result<LoginCommandResponse>>;

internal sealed class LoginWithTFACommandHandler(
    IUserRepository userRepository,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginWithTFACommand, Result<LoginCommandResponse>>
{
    public async Task<Result<LoginCommandResponse>> Handle(LoginWithTFACommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p =>
            p.Email.Value == request.EmailOrUserName
            || p.UserName.Value == request.EmailOrUserName);

        if (user is null)
        {
            return Result<LoginCommandResponse>.Failure("Kullanıcı adı ya da şifre yanlış.");
        }
        if (user.TFAIsCompleted is null || user.TFAExpiresDate is null || user.TFACode is null || user.TFAConfirmCode is null)
        {
            return Result<LoginCommandResponse>.Failure("TFA Kodu geçersiz.");
        }
        if (user.TFAIsCompleted.Value)
        {
            return Result<LoginCommandResponse>.Failure("TFA Kodu geçersiz.");
        }
        if (user.TFAExpiresDate.Value < DateTimeOffset.Now)
        {
            return Result<LoginCommandResponse>.Failure("TFA Kodu geçersiz.");
        }
        if( user.TFAConfirmCode.Value != request.TFAConfirmCade || user.TFACode.Value != request.TFACode)
        {
            return Result<LoginCommandResponse>.Failure("TFA Kodu geçersiz.");
        }

        user.SetTFACompleted();
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = await jwtProvider.CreateTokenAsync(user, cancellationToken);
        var res = new LoginCommandResponse() { Token = token };
        return res;
    }
}
