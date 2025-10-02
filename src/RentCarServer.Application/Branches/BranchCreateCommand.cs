using FluentValidation;
using GenericRepository;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Branchs.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Branchs;

public sealed record BranchCreateCommand(
    string Name,
    Address Address,
    bool IsActive) : IRequest<Result<string>>;

public sealed class BranchCreateCommandValidator : AbstractValidator<BranchCreateCommand>
{
    public BranchCreateCommandValidator()
    {
        RuleFor(i => i.Name).NotEmpty().WithMessage("Geçerli bir şube adı giriniz.");
        RuleFor(i => i.Address.City).NotNull().WithMessage("Geçerli bir şehir giriniz.");
        RuleFor(i => i.Address.District).NotNull().WithMessage("Geçerli bir ilçe giriniz.");
        RuleFor(i => i.Address.FullAddress).NotNull().WithMessage("Geçerli bir tam adres giriniz.");
        RuleFor(i => i.Address.PhoneNumber1).NotNull().WithMessage("Geçerli bir telefon numarası giriniz.");
    }
}

internal sealed class BrancCreateCommandHandler(
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<BranchCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(BranchCreateCommand request, CancellationToken cancellationToken)
    {
        var nameIsExist = await branchRepository.AnyAsync(p => p.Name.Value == request.Name, cancellationToken);
        if (nameIsExist)
            return Result<string>.Failure("Bu isimde bir şube zaten mevcut.");

        Name name = new(request.Name);
        Address address = request.Address;
        Branch branch = new(name, address, request.IsActive);
        branchRepository.Add(branch);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Şube başarıyla oluşturuldu.";
    }
}