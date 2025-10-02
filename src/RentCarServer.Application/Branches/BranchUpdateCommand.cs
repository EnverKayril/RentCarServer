using FluentValidation;
using GenericRepository;
using RentCarServer.Application.Branchs;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Branchs.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Branches;

public sealed record BranchUpdateCommand(
    Guid Id, 
    string Name, 
    Address Address,
    bool IsActive) : IRequest<Result<string>>;

public sealed class BranchUpdateCommandValidator : AbstractValidator<BranchUpdateCommand>
{
    public BranchUpdateCommandValidator()
    {
        RuleFor(i => i.Name).NotEmpty().WithMessage("Geçerli bir şube adı giriniz.");
        RuleFor(i => i.Address.City).NotNull().WithMessage("Geçerli bir şehir giriniz.");
        RuleFor(i => i.Address.District).NotNull().WithMessage("Geçerli bir ilçe giriniz.");
        RuleFor(i => i.Address.FullAddress).NotNull().WithMessage("Geçerli bir tam adres giriniz.");
        RuleFor(i => i.Address.PhoneNumber1).NotNull().WithMessage("Geçerli bir telefon numarası giriniz.");
    }
}

internal sealed class BranchCommandHandler(
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<BranchUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(BranchUpdateCommand request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (branch is null)
            return Result<string>.Failure("Güncellemek istediğiniz şube bulunamadı.");

        Name name = new(request.Name);
        Address address = request.Address;
        branch.SetName(name);
        branch.SetAddress(address);
        branch.SetStatus(request.IsActive);
        branchRepository.Update(branch);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return "Şube bilgisi başarıyla güncellendi.";
    }
}