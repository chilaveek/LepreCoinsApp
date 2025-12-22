namespace Interfaces.Service
{
    /// <summary>
    /// Интерфейс для управления аутентификацией и текущим пользователем
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Получить ID текущего пользователя
        /// </summary>
        int GetCurrentUserId();

        /// <summary>
        /// Установить ID текущего пользователя
        /// </summary>
        void SetCurrentUserId(int userId);

        /// <summary>
        /// Проверить, авторизован ли пользователь
        /// </summary>
        bool IsAuthenticated();

        /// <summary>
        /// Выйти (очистить данные пользователя)
        /// </summary>
        void Logout();
    }
}