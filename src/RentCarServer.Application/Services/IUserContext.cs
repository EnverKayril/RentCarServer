namespace RentCarServer.Application.Services;

/// <summary>
/// Geçerli kullanıcı hakkında bilgi almak için bir bağlamı temsil eder.
/// </summary>
/// <remarks>
/// Bu arayüz, geçerli bağlamla ilişkili kullanıcının benzersiz kimliğini elde etmek için bir yöntem sağlar. 
/// Genellikle kullanıcıya özel işlemlerin veya verilerin gerektiği senaryolarda kullanılır.
/// </remarks>
public interface IUserContext
{
    Guid GetUserId();
}
