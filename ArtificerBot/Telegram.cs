using Artificer.Bot.Buttons;using Artificer.Bot.Commands;using Artificer.Bot.Handlers;using Telegram.Bot;using Telegram.Bot.Exceptions;using Telegram.Bot.Polling;using Telegram.Bot.Types;using Telegram.Bot.Types.Enums;using Telegram.Bot.Types.ReplyMarkups;using Exception = System.Exception;using Artificer.Utility;using Artificer.Utility.Keyboards;using NAudio.Wave;using Newtonsoft.Json;using Telegram.Bot.Types.InputFiles;using TL;using WTelegram;using File = System.IO.File;using Task = System.Threading.Tasks.Task;using TMessage = Telegram.Bot.Types.Message;using Update = Telegram.Bot.Types.Update;#pragma warning disable CS8618namespace Artificer.Bot;public class Telegram : IAsyncInitialization{    private TelegramBotClient _client;    private CancellationTokenSource _cts;    private Client _agent;        public Telegram()    {        Initialization = InitializeAsync();    }    public Task Initialization { get; }    private async Task InitializeAsync()    {        Debug.Log("Initializing an instance of the Telegram API...", Debug.Sender.Telegram, Debug.MessageStatus.INFO);        _client = new TelegramBotClient(Kernel.Settings.TgAccessToken);        _cts = new CancellationTokenSource();        var receiverOptions = new ReceiverOptions        {            AllowedUpdates = Array.Empty<UpdateType>()        };        _client.StartReceiving(            updateHandler: HandleUpdateAsync,            pollingErrorHandler: HandlePollingErrorAsync,            receiverOptions: receiverOptions,            cancellationToken: _cts.Token        );        Helpers.Log = (i, s) =>        {            switch (i)            {                case 3:                    Debug.Log($"[TelegramAgent] {s}", Debug.Sender.Telegram, Debug.MessageStatus.WARN);                    break;                case 4:                    Debug.Log($"[TelegramAgent] {s}", Debug.Sender.Telegram, Debug.MessageStatus.FAIL);                    break;            }        };        _agent = await TelegramAgentInit();        Debug.Log("The Telegram API instance has been initialized and authorized on the server.", Debug.Sender.Telegram, Debug.MessageStatus.INFO);    }    private async Task<Client> TelegramAgentInit()    {        var client = new Client(Kernel.Settings.AppId, Kernel.Settings.AppHash); // this constructor doesn't need a Config method        await DoLogin(Kernel.Settings.Telephone); // initial call with user's phone_number        async Task DoLogin(string loginInfo) // (add this method to your code)        {            while (client.User == null)                switch (await client.Login(loginInfo)) // returns which config is needed to continue login                {                    case "verification_code": Console.Write("Enter code: "); loginInfo = Console.ReadLine()!; break;                    case "name": loginInfo = Kernel.Settings.FirstName; break;    // if sign-up is required (first/last_name)                    case "password": loginInfo = Kernel.Settings.CloudPassword; break; // if user has enabled 2FA                    default: loginInfo = null!; break;                }        }                client.OnUpdate += AgentOnUpdate;        return client;    }    private async Task AgentOnUpdate(IObject arg)    {        if (arg is not UpdatesBase updates) return;        foreach (var update in updates.UpdateList)        {            switch (update)            {                case UpdateNewMessage unm:                    switch (unm.message)                    {                        case TL.Message message:                             await AgentMessage.HandleAsync(_agent, message);                             break;                    }                    break;            }        }    }    private static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)    {        async void Start()        {            try            {                if (update.Message is {Text: { }} message)                {                    if (update.Message.From!.Id != Kernel.Settings.AgentId)                    {                        //Если обновление не в личке бота, скипаем его                        if (update.Message.Chat.Id != update.Message.From!.Id) return;                        var chatId = message.Chat.Id;                        var user = Kernel.Users.GetUserFromChatId(chatId);                        //await botClient.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken);                        //Обработчик команд                        if (await CommandQueryHandleAsync(botClient, user!, chatId, message.Text, cancellationToken)) return;                        //Обработка нажатий клавиш на основной клавиатуре                        if (await ButtonQueryHandleAsync(botClient, user!, message.Text, cancellationToken)) return;                        //Проверяем, не ожидает ли бот имени организации                        var organization = Kernel.Organizations.GetOrganizationFromManager(user!.ID);                        if (organization is {Stage: OrganizationStage.SetName})                        {                            await OrganizationNameValidation.HandleAsync(botClient, user, organization, message.Text,                                cancellationToken);                            return;                        }                        await RequestQuery.HandleAsync(botClient, user, message.Text, cancellationToken);                    }                    else                    {                        var json = JsonConvert.DeserializeObject<dynamic>(message.Text);                        long from = Convert.ToInt64(json["from"].ToString());                        string path = json["path"].ToString();                        int seconds = Convert.ToInt32(json["seconds"].ToString());                        int waitMessage = Convert.ToInt32(json["wait"].ToString());                        int reply = Convert.ToInt32(json["reply"].ToString());                        var user = Kernel.Users.GetUserFromChatId(from);                        await using (var fileStream = new FileStream(path, FileMode.Open))                        {                            var inputFile = new InputOnlineFile(fileStream)                            {                                FileName = Path.GetFileName(path)                            };                            await botClient.DeleteMessageAsync(new ChatId(from), waitMessage,                                cancellationToken: cancellationToken);                            var text = "Я расшифровал для вас аудиозапись в текстовый файл";                                                        Debug.Log($"[RESPONS] {text}", user!.ID);                            await botClient.SendDocumentAsync(new ChatId(from), inputFile,                                caption: text, replyToMessageId: reply,                                cancellationToken: cancellationToken);                            #pragma warning disable CS4014                            Kernel.Database.AddWhisperRequest(user.ID, user.Organization, seconds);                            #pragma warning restore CS4014                            Kernel.UpdateWhisperBalance(user, seconds);                        }                        var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");                        var pathOutput = Path.Combine(tempPath, Path.GetDirectoryName(path)!);                        Directory.Delete(pathOutput, true);                    }                }                if (update.Message?.Contact != null)                {                    var chatId = update.Message.Chat.Id;                    var user = Kernel.Users.GetUserFromChatId(chatId);                    var contact = update.Message.Contact;                    if (user == null) return;                    user.Telephone = contact.PhoneNumber.Replace("+", "");                    user.FirstName = contact.FirstName;                    user.LastName = contact.LastName;                    user.Trial = false;                    user.Stage = UserStage.Ready;                    await botClient.DeleteMessageAsync(new ChatId(user.ID), update.Message.MessageId,                        cancellationToken: cancellationToken);                    if (user.IsManager)                    {                        var organization = Kernel.Organizations.GetOrganizationFromManager(user.ID);                        if (organization!.Type == OrganizationType.Balance)                        {                            organization.Stage = OrganizationStage.Work;                            var msg = Kernel.Messages.Get("organization_balance_success");                            await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: UserKeyboard.GetKeyboard(),                                cancellationToken: cancellationToken);                            Debug.Log($"[MESSAGE] {msg}", user.ID);                            await Kernel.Database.AddAction(user.ID, ActionType.AddOrganization);                        }                        if (organization.Type == OrganizationType.Contract)                        {                            organization.Stage = OrganizationStage.WaitContract;                            var msg = Kernel.Messages.Get("organization_contract_success");                            await botClient.SendTextMessageAsync(user.ID,                                Kernel.Messages.Get("organization_contract_success"),                                replyMarkup: UserKeyboard.GetKeyboard(), cancellationToken: cancellationToken);                            Debug.Log($"[MESSAGE] {msg}", user.ID);                            await Kernel.Database.AddAction(user.ID, ActionType.AddOrganization);                        }                    }                    else if (user.TempData != null)                    {                        await InviteCommand.HandleAsync(botClient, user, user.TempData, cancellationToken);                    }                    else                    {                        var msg = Kernel.Messages.Get("user_success");                        await botClient.SendTextMessageAsync(user.ID, Kernel.Messages.Get("user_success"),                            replyMarkup: UserKeyboard.GetKeyboard(), cancellationToken: cancellationToken);                        Debug.Log($"[MESSAGE] {msg}", user.ID);                    }                }                //Обработка нажатий на инлайн клавиатуру                if (update.CallbackQuery is {Data: { }} query)                {                    var user = Kernel.Users.GetUserFromChatId(query.From.Id);                    await CallbackQueryHandleAsync(botClient, user!, query.Data, cancellationToken);                }                if (update.Message is {Voice: { }} voice)                {                    await WhisperRequestQuery.HandleAsync(botClient, voice, cancellationToken);                }                if (update.Message is {Audio: { }} audioMessage)                {                    var chatId = audioMessage.Chat.Id;                    var user = Kernel.Users.GetUserFromChatId(chatId);                    var ext = Path.GetExtension(audioMessage.Audio.FileName);                    if (ext is ".mp3" or ".ogg" or ".m4a" or ".opus" or ".aac")                    {                        await WhisperQuery.HandleAudioAsync(botClient, user!, audioMessage, ext, cancellationToken);                    }                    else                        await botClient.SendTextMessageAsync(user!.ID,                            Kernel.Messages.Get("format_error").Replace("{%EXT%}", ext),                            replyToMessageId: audioMessage.MessageId, cancellationToken: cancellationToken);                }                if (update.Message is {Document: { }} documentMessage)                {                    var chatId = documentMessage.Chat.Id;                    var user = Kernel.Users.GetUserFromChatId(chatId);                    var ext = Path.GetExtension(documentMessage.Document.FileName);                    if (ext is ".wav" or ".aac" or ".mp4" or ".mpeg" or ".webm")                    {                        await WhisperQuery.HandleDocumentAsync(botClient, user!, documentMessage, ext,                            cancellationToken);                    }                    else                        await botClient.SendTextMessageAsync(user!.ID,                            Kernel.Messages.Get("format_error").Replace("{%EXT%}", ext),                            replyToMessageId: documentMessage.MessageId, cancellationToken: cancellationToken);                }                if (update.Message is {Video: { }} videoMessage)                {                    var chatId = videoMessage.Chat.Id;                    var user = Kernel.Users.GetUserFromChatId(chatId);                    //var ext = Path.GetExtension(videoMessage.Video.FileName);                    /*if (ext is".mp4")                    {                        await WhisperQuery.HandleVideoAsync(botClient, user!, videoMessage, ext, cancellationToken);                    }                    else*/                    await botClient.SendTextMessageAsync(user!.ID, "❌ Распознование видеофайлов пока не поддерживается",                        replyToMessageId: videoMessage.MessageId, cancellationToken: cancellationToken);                }                if (update.MyChatMember != null)                {                    switch (update.MyChatMember.NewChatMember.Status)                    {                        case ChatMemberStatus.Kicked:                        {                            var user = Kernel.Users.GetUserFromChatId(update.MyChatMember.Chat.Id);                            if (user != null)                            {                                user.Enabled = false;                                await Kernel.Database.AddAction(user.ID, ActionType.UserLeave);                            }                            break;                        }                        case ChatMemberStatus.Member:                        {                            var user = Kernel.Users.GetUserFromChatId(update.MyChatMember.Chat.Id);                            if (user != null)                            {                                user.Enabled = true;                                await Kernel.Database.AddAction(user.ID, ActionType.UserReturn);                            }                            break;                        }                        case ChatMemberStatus.Creator:                            break;                        case ChatMemberStatus.Administrator:                            break;                        case ChatMemberStatus.Left:                            break;                        case ChatMemberStatus.Restricted:                            break;                        default:                            throw new ArgumentOutOfRangeException();                    }                }            }            catch (Exception e)            {                if (e.Message == "Forbidden: bot was blocked by the user")                {                    Kernel.Users.GetUserFromChatId(update.Message?.From?.Id)!.Enabled = false;                }                else Debug.Log($"[HandleUpdateAsync] {e.Message}: {e.StackTrace}", Debug.Sender.Telegram,                    Debug.MessageStatus.FAIL);            }        }        new Thread(Start).Start();                return Task.CompletedTask;    }    private static async Task CallbackQueryHandleAsync(ITelegramBotClient botClient, User user, string data, CancellationToken cancellationToken)    {        try        {            if (data.StartsWith("price"))            {                await Callbacks.PriceCallback.HandleAsync(botClient, user, Convert.ToInt32(data.Split(' ')[1]), cancellationToken);            }            else if (data.StartsWith("user"))            {                await Callbacks.UserCallback.HandleAsync(botClient, user, Convert.ToInt32(data.Split(' ')[1]), cancellationToken);            }            else if (data.StartsWith("organization"))            {                await Callbacks.OrganizationCallback.HandleAsync(botClient, user, Convert.ToInt32(data.Split(' ')[1]), cancellationToken);            }            else if (data.StartsWith("contract"))            {                await Callbacks.ContractCallback.HandleAsync(botClient, user, Convert.ToInt32(data.Split(' ')[1]), cancellationToken);            }            else if (data.StartsWith("balance"))            {                await Callbacks.BalanceCallback.HandleAsync(botClient, user, Convert.ToInt32(data.Split(' ')[1]), cancellationToken);            }            else if (data.StartsWith("registration"))            {                await Callbacks.RegistrationCallback.HandleAsync(botClient, user, Convert.ToInt32(data.Split(' ')[1]), cancellationToken);            }            else if (data == "payment_error_keyboard")            {                await PaymentButton.HandleAsync(botClient, user, cancellationToken);            }        }        catch (Exception e)        {            Debug.Log($"[CallbackQueryHandleAsync] {e.Message}: {e.StackTrace}", Debug.Sender.Telegram, Debug.MessageStatus.FAIL);        }    }        private static async Task<bool> CommandQueryHandleAsync(ITelegramBotClient botClient, User user, long chatId, string? data, CancellationToken cancellationToken)    {        try        {            //await botClient.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken);                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse            if (user == null || (data == "/start" && user == null))            {                await StartCommand.HandleAsync(botClient, chatId, cancellationToken);                return true;            }            // ReSharper disable once ConditionIsAlwaysTrueOrFalse            if (data == "/start" && user != null)            {                await RequestQuery.HandleAsync(botClient, user, "Привет!", cancellationToken);                return true;            }            if (data == "/clearkeyboard")            {                await botClient.SendTextMessageAsync(user!.ID, "✅ Клавиатура успешно удалена", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);                return true;            }            if (data == "/updatekeyboard")            {                await botClient.SendTextMessageAsync(user!.ID, "✅ Клавиатура успешно обновлена", replyMarkup: UserKeyboard.GetKeyboard(), cancellationToken: cancellationToken);                return true;            }            if (data == "/price")            {                await botClient.SendTextMessageAsync(user!.ID, Kernel.Messages.Get("price"), ParseMode.Markdown, cancellationToken: cancellationToken);                return true;            }            if (data!.StartsWith("/"))            {                await botClient.SendTextMessageAsync(user!.ID, "❌ Команда не распознана", cancellationToken: cancellationToken);                return true;            }                    //Обработка инвайта            if (data.StartsWith("Для того, чтобы добавить сотрудника в свою организацию, просто перешлите ему это сообщение") || data.StartsWith("Invite:"))            {                if (user?.Organization == null && user?.Telephone == null)                {                    user!.TempData = data;                    await botClient.SendTextMessageAsync(user.ID, Kernel.Messages.Get("employee_contact"), replyMarkup: GetContactKeyboard.GetKeyboard(),                        cancellationToken: cancellationToken);                }                else await InviteCommand.HandleAsync(botClient, user, data, cancellationToken);                return true;            }            if (user!.Stage == UserStage.Paymant)            {                await PaymentCommand.HandleAsync(botClient, user, data, cancellationToken);                return true;            }            if (user.Stage == UserStage.Titles)            {                await SocialmediaCommand.HandleAsync(botClient, user,                     Kernel.Messages.Get("title_prompt").Replace("{%DATA%}", data), cancellationToken);                return true;            }            if (user.Stage == UserStage.Hash)            {                await SocialmediaCommand.HandleAsync(botClient, user,                     Kernel.Messages.Get("hash_prompt").Replace("{%DATA%}", data), cancellationToken);                return true;            }                        if (user.Stage == UserStage.Sinonyms)            {                await SocialmediaCommand.HandleAsync(botClient, user,                     Kernel.Messages.Get("sinonyms_prompt").Replace("{%DATA%}", data), cancellationToken);                return true;            }                        if (user.Stage == UserStage.Keywords)            {                await SocialmediaCommand.HandleAsync(botClient, user,                     Kernel.Messages.Get("keywords_prompt").Replace("{%DATA%}", data), cancellationToken);                return true;            }        }        catch (Exception e)        {            Debug.Log($"[CommandQueryHandleAsync] {e.Message}: {e.StackTrace}", Debug.Sender.Telegram, Debug.MessageStatus.FAIL);        }        return false;    }    private static async Task<bool> ButtonQueryHandleAsync(ITelegramBotClient botClient, User user, string data, CancellationToken cancellationToken)    {        try        {            switch (data)            {                case SettingsKeyboard.STATUS_BUTTON:                    await StatusButton.HandlerAsync(botClient, user, cancellationToken);                    return true;                case SettingsKeyboard.ADD_EMPLOYEE_BUTTON:                    await AddEmployeeButton.HandlerAsync(botClient, user, cancellationToken);                    return true;                case SettingsKeyboard.DEL_EMPLOYEE_BUTTON:                    await DevelopButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case SettingsKeyboard.HELP_BUTTON:                    await HelpButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case SettingsKeyboard.MANAGER_BUTTON:                    await ManagerButton.HandleAsync(botClient, user.ID, cancellationToken);                    return true;                case SettingsKeyboard.BALANCE_BUTTON:                    await PaymentButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case SettingsKeyboard.ISSUE_INVOICE:                    await DevelopButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case UserKeyboard.SCENARIOS_BUTTON:                    await ScenariosButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case ScenariosKeyboard.MAINMENU_BUTTON or SettingsKeyboard.MAINMENU_BUTTON:                    await MainmenuButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case ScenariosKeyboard.TITLES_BUTTON:                    await TitlesButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case ScenariosKeyboard.HASHTAG_BUTTON:                    await HashtagButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case ScenariosKeyboard.SINONYMS_BUTTON:                    await SinonymsButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case ScenariosKeyboard.KEYWORDS_BUTTON:                    await KeywordsButton.HandleAsync(botClient, user, cancellationToken);                    return true;                /*case ScenariosKeyboard.AUDIOTOTEXT_BUTTON:                    await AudioButton.HandleAsync(botClient, user, cancellationToken);                    return true;*/                case UserKeyboard.SETTINGS_BUTTON:                    await SettingsButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case SettingsKeyboard.DEL_ORGANIZATION_BUTTON:                    await DelOrganizationButton.HandleAsync(botClient, user, cancellationToken);                    return true;                case SettingsKeyboard.REGISTRATION_BUTTON:                    var del = await botClient.SendTextMessageAsync(user.ID, "💬", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);                    await  botClient.DeleteMessageAsync(new ChatId(user.ID), del.MessageId, cancellationToken: cancellationToken);                    await Callbacks.RegistrationCallback.HandleAsync(botClient, user, cancellationToken);                    return true;            }        }        catch (Exception e)        {            Debug.Log($"[ButtonQueryHandleAsync] {e.Message}: {e.StackTrace}", Debug.Sender.Telegram, Debug.MessageStatus.FAIL);            return true;        }        return false;    }    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)    {        var errorMessage = exception switch        {            ApiRequestException apiRequestException                => $"[HandlePollingErrorAsync] {apiRequestException.Message} [{apiRequestException.ErrorCode}] ({apiRequestException.StackTrace})",            _ => exception.ToString()        };        Debug.Log(errorMessage, Debug.Sender.Telegram, Debug.MessageStatus.FAIL);                return Task.CompletedTask;    }    public async Task SendBroadcastMessageAsync(string message)    {        long id = 0;        try        {            foreach (var user in Kernel.Users.GetUserList)            {                id = user.ID;                if (user.Enabled)                {                    await _client.SendTextMessageAsync(user.ID, message, ParseMode.Markdown, replyMarkup: UserKeyboard.GetKeyboard());                                        Debug.Log($"[MESSAGE] {message}", user.ID);                }                                    await Task.Delay(TimeSpan.FromSeconds(0.3d));            }        }        catch (Exception e)        {            if (e.Message == "Forbidden: bot was blocked by the user")            {                Kernel.Users.GetUserFromChatId(id)!.Enabled = false;            }        }    }    public async Task SendMessageFromUserId(long userId, string message, IReplyMarkup? replyMarkup = null)    {        var user = Kernel.Users.GetUserFromChatId(userId);        if (user!.Enabled)        {            if (replyMarkup != null)            {                await _client.SendTextMessageAsync(userId, message, replyMarkup: replyMarkup, cancellationToken: _cts.Token);            }            else await _client.SendTextMessageAsync(userId, message, cancellationToken: _cts.Token);        }    }        public static async Task SendWaitMessage(ITelegramBotClient botClient, User user, int millisecondsDelay, string message, CancellationToken cancellationToken)    {        try        {            await botClient.SendTextMessageAsync(user.ID, message, cancellationToken: cancellationToken);            while (millisecondsDelay > 5000)            {                await botClient.SendChatActionAsync(user.ID, ChatAction.Typing, cancellationToken);                await Task.Delay(5000, cancellationToken);                millisecondsDelay -= 5000;            }            await botClient.SendChatActionAsync(user.ID, ChatAction.Typing, cancellationToken);            await Task.Delay(millisecondsDelay, cancellationToken);        }        catch (Exception e)        {            if (e.Message.StartsWith("Forbidden: bot was blocked by the user"))            {                user.Enabled = false;            }            else            {                Debug.Log($"[SendWaitMessage] {e.Message}: {e.StackTrace}", Debug.Sender.Telegram, Debug.MessageStatus.FAIL);            }        }            }}