using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NrExtras.TelegramHelper
{
    public class TelegramCommandListener
    {
        private readonly TelegramBotClient _botClient;
        private readonly ILogger? _logger;
        private Func<string, long, Task>? _onCommandReceived;
        private CancellationTokenSource _cts;
        private long _chatId = 0;

        public TelegramCommandListener(string botToken, long chatId, ILogger? logger = null)
        {
            _botClient = new TelegramBotClient(botToken);
            _chatId = chatId;
            _logger = logger;
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Handle errors from the Telegram Bot API
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiEx => $"Telegram API Error: [{apiEx.ErrorCode}] {apiEx.Message}",
                _ => $"Unexpected Error: {exception.Message}"
            };

            Log(exception, "Telegram API Error");
            return Task.CompletedTask;
        }
        #region Listen
        /// <summary>
        /// Start listening for incoming messages from Telegram
        /// </summary>
        /// <param name="onCommandReceived"></param>
        public void StartListening(Func<string, long, Task> onCommandReceived)
        {
            _onCommandReceived = onCommandReceived;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: _cts.Token
            );

            Log("Telegram listener started.", LogLevel.Information);
        }

        /// <summary>
        /// Stop listening for incoming messages
        /// </summary>
        public void StopListening()
        {
            _cts.Cancel();
            Log("Telegram listener stopped.", LogLevel.Information);
        }

        /// <summary>
        /// Handle incoming updates from Telegram
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message || update.Message?.Text == null)
                return;

            var message = update.Message;
            Log($"Received message: '{message.Text}' from chat {message.Chat.Id}", LogLevel.Debug);

            if (_onCommandReceived != null)
                await _onCommandReceived(message.Text, message.Chat.Id);
        }
        #endregion
        #region Logging
        /// <summary>
        /// Log messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level">default=info</param>
        private void Log(string message, LogLevel level = LogLevel.Information, Exception? exception = null)
        {
            if (_logger != null)
            {
                //Log exception if not null
                if (exception != null)
                    _logger.Log(level, exception, message);
                else //just log the message - no exception
                    _logger.Log(level, message);
            }
            else
            {
                var prefix = level switch
                {
                    LogLevel.Debug => "[Debug]",
                    LogLevel.Error => "[Error]",
                    LogLevel.Warning => "[Warn]",
                    LogLevel.Information => "[Info]",
                    LogLevel.Critical => "[Critical]",
                    _ => "[Log]"
                };
                Console.WriteLine($"{prefix} {message}");
            }
        }

        /// <summary>
        /// Log messages with exception
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        private void Log(Exception exception, string message)
        {
            Log(message, LogLevel.Error, exception);
        }
        #endregion
        #region SendMessages
        /// <summary>
        /// Send a message to a specific chat
        /// </summary>
        /// <param name="chatId">The chat ID where the message will be sent</param>
        /// <param name="message">The message to send</param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message, long? chatId = null)
        {
            // check if chatId is null, if so use the default chatId we recieived in the constructor
            if (chatId == null) chatId = _chatId;

            try
            {
                await _botClient.SendMessage(chatId, message);
                Log($"Sent message to chat {chatId}: {message}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Log(ex, $"Error sending message to chat {chatId}: {ex.Message}"); // Log the exception
            }
        }

        /// <summary>
        /// Send a photo to a specific chat using pngBytes
        /// </summary>
        /// <param name="pngBytes"></param>
        /// <param name="chatId"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public async Task SendPhotoAsync(byte[] pngBytes, long? chatId = null, string caption = "")
        {
            if (chatId == null) chatId = _chatId;

            try
            {
                using var stream = new MemoryStream(pngBytes);
                var inputFile = InputFile.FromStream(stream, "chart.png");

                await _botClient.SendPhoto(
                    chatId: chatId,
                    photo: inputFile,
                    caption: caption
                );
            }
            catch (Exception ex)
            {
                Log(ex, $"Error sending photo to chat {chatId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Send a photo to a specific chat using file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="chatId"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public async Task SendPhotoAsync(string filePath, long? chatId = null, string caption = "")
        {
            if (chatId == null) chatId = _chatId;

            try
            {
                // Open the file as a stream
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                // Create an InputFile from the file stream
                var inputFile = InputFile.FromStream(fileStream, Path.GetFileName(filePath));

                // Send the photo
                await _botClient.SendPhoto(
                    chatId: chatId,
                    photo: inputFile,
                    caption: caption
                );
            }
            catch (Exception ex)
            {
                Log(ex, $"Error sending photo to chat {chatId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Send a photo to a specific chat using MemoryStream
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="chatId"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public async Task SendPhotoAsync(MemoryStream memoryStream, long? chatId = null, string caption = "")
        {
            if (chatId == null) chatId = _chatId;

            try
            {
                var inputFile = InputFile.FromStream(memoryStream, "chart.png");

                await _botClient.SendPhoto(
                    chatId: chatId,
                    photo: inputFile,
                    caption: caption
                );
            }
            catch (Exception ex)
            {
                Log(ex, $"Error sending photo to chat {chatId}: {ex.Message}");
            }
        }
        #endregion
    }
}