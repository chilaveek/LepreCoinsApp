namespace LC.BLL.Services
{
    using Interfaces.Service;
    using System;

    /// <summary>
    /// Сервис для управления аутентификацией и текущим пользователем
    /// Хранит ID пользователя в памяти приложения (Singleton)
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private int _currentUserId = 1;
        private bool _isAuthenticated = false;

        /// <summary>
        /// Получить ID текущего авторизованного пользователя
        /// </summary>
        /// <returns>ID пользователя или 0, если не авторизован</returns>
        public int GetCurrentUserId()
        {
            return _currentUserId;
        }

        /// <summary>
        /// Установить текущего пользователя по его ID
        /// Вызывается после успешной аутентификации (например, при входе)
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <example>
        /// // После успешного входа:
        /// var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
        /// authService.SetCurrentUserId(user.Id);
        /// </example>
        public void SetCurrentUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID должен быть больше 0", nameof(userId));

            _currentUserId = userId;
            _isAuthenticated = true;
        }

        /// <summary>
        /// Проверить, авторизован ли пользователь в системе
        /// </summary>
        /// <returns>true, если пользователь авторизован; иначе false</returns>
        public bool IsAuthenticated()
        {
            return _isAuthenticated && _currentUserId > 0;
        }

        /// <summary>
        /// Выйти из аккаунта (очистить данные пользователя)
        /// </summary>
        /// <example>
        /// // При выходе:
        /// var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
        /// authService.Logout();
        /// // Навигация на страницу входа
        /// await Shell.Current.GoToAsync("login");
        /// </example>
        public void Logout()
        {
            _currentUserId = 0;
            _isAuthenticated = false;
        }
    }
}