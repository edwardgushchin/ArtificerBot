using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Artificer.Utility;
using NAudio.Wave;
using Telegram.Bot.Types.ReplyMarkups;
using Debug = Artificer.Utility.Debug;

namespace Artificer.Bot;

public static class Kernel
{
#pragma warning disable CS8618
    private static Setting _settings;
    private static Telegram _telegram;
    private static Organizations _organizations;
    private static Users _users;
    private static RequestManager _requestManager;
    private static Invites _invites;
    private static Database _database;
    private static Payments _payments;
    private static Messages _messages;
    private static Prompts _prompts;
    private static StatisticManager _statisticManager;
    private static WebProxy _proxy;
    public static readonly bool IsDebug = false;
#pragma warning restore CS8618

    public static async Task Main()
    {
        Debug.Message("ArtificerBot Bot version 1.5 beta");
        Debug.NewLine();
        Debug.Message("Copyright © 2023 Edward Gushchin");
        Debug.Message("Licensed under the Apache License, Version 2.0");
        Debug.NewLine();
        Debug.Message("Need a bot? For you here: https://t.me/eduardgushchin");
        Debug.NewLine();
        Debug.Message($"Work folder: {AppDomain.CurrentDomain.BaseDirectory}");
        Debug.NewLine();

        var logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        var updatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updates");
        var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
        var promptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prompts");
        var reportsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");

        if (!Path.Exists(logsPath)) Directory.CreateDirectory(logsPath);
        if (!Path.Exists(updatesPath)) Directory.CreateDirectory(updatesPath);
        if (!Path.Exists(tempPath)) Directory.CreateDirectory(tempPath);
        if (!Path.Exists(promptsPath)) Directory.CreateDirectory(promptsPath);
        if (!Path.Exists(Path.Combine(logsPath, "users"))) Directory.CreateDirectory(Path.Combine(logsPath, "users"));
        
        _settings = new Setting();
        _messages = new Messages();
        _prompts = new Prompts();

        if (!_settings.IsInitialized) return;
        
        Debug.Log("Initializing the database subsystem...", Debug.Sender.Kernel, Debug.MessageStatus.INFO);

        _database = new Database();
        await _database.Initialization;

        _invites = new Invites();
        _organizations = new Organizations();
        _users = new Users();
        _requestManager = new RequestManager();
        _statisticManager = new StatisticManager(reportsPath);
        _payments = new Payments();

        Debug.Log("Database subsystem initialized successfully!", Debug.Sender.Kernel, Debug.MessageStatus.INFO);

        _telegram = new Telegram();
        await _telegram.Initialization;

        _proxy = new WebProxy($"socks5://{_settings.Proxy}")
        {
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false,

            // *** These creds are given to the proxy server, not the web server ***
            Credentials = new NetworkCredential(
                userName: _settings.ProxyLogin,
                password: _settings.ProxyPass)
        };
        
        Debug.PrintConsole = false;

        Debug.NewLine();

        var loop = true; 
        while (loop)
        {
            try
            {
                Console.Write("artificer> ");
                var command = Console.ReadLine();

                if (command == "messages update")
                {
                    _messages = new Messages();
                    Console.WriteLine($"Messages sussesful update.");
                    Console.WriteLine();
                }
                else if (command == "list organizations")
                {
                    var list = Organizations.OrganizationsList;
                    for (var i = 0; i < list.Length; i++)
                    {
                        Console.WriteLine($"{i}: {list[i].Name}");
                    }

                    Console.WriteLine();
                }
                else if (command!.StartsWith("set rate"))
                {
                    var split = command.Split(' ');
                    var id = int.Parse(split[2]);
                    var value = (OrganizationRate)int.Parse(split[3]);
                    var organization = Organizations.OrganizationsList[id];
                    organization.Rate = value;
                    if (organization.Stage == OrganizationStage.WaitContract)
                        organization.Stage = OrganizationStage.Work;
                }
                else if (command.StartsWith("add balance"))
                {
              
                    var split = command.Split(' ');
                    var id = int.Parse(split[2]);
                    var count = int.Parse(split[3]);
                    var user = Users.GetUserFromChatId(id);
                    user!.Balance += count;
                    var message = $"✅ Администрация проекта начислила вам {count} бонусных рублей!";
                    _telegram.SendMessageFromUserId(user.ID, message).GetAwaiter().GetResult();
                    Debug.Log(message, user.ID);
                }
                else if (command == "add stat")
                {
                    await _statisticManager.AddStatistic();
                }
                else if (command == "update reports")
                {
                    _statisticManager.UpdateReports();
                }
                else if (command.StartsWith("update"))
                {
                    var split = command.Split(' ');
                    var file = await File.ReadAllTextAsync(Path.Combine(updatesPath, $"{split[1]}.md"));
                    #pragma warning disable CS4014
                    await _telegram.SendBroadcastMessageAsync(file);
                    #pragma warning restore CS4014
                    Debug.Log($"Message {split[1]} succesfull sended.", Debug.Sender.Kernel, Debug.MessageStatus.WARN);
                }
                else if (command == "exit")
                {
                    Task.WaitAll();
                    loop = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message} {e.StackTrace}");
            }

        }
    }

    private static bool CheckBalanceForOrganization(User user, out string? errorMessage)
    {
        var organization = Organizations.GetOrganizationFromManager(user.Organization);
        var manager = user.IsManager ? user : Users.GetUserFromChatId(organization!.Manager);

        //Если тип организации по расчету
        if (organization!.Type == OrganizationType.Balance)
        {
            //Если нет денег на балансе менеджера
            if (manager!.Balance <= 0)
            {
                errorMessage = manager.IsManager
                    ? "❌ На счете вашей организации недостаточно средств. Пожалуйста, пополните счет"
                    : "❌ На счете вашей организации недостаточно средств";
                return false;
            }
        }

        //Если тип организации по контракту и организация ожидает заключение договора
        if (organization is {Type: OrganizationType.Contract, Stage: OrganizationStage.WaitContract})
        {
            errorMessage = manager!.IsManager
                ? "❌ Ваша организация не обслуживается. Пожалуйста, напишите нашему менеджеру для заключения договора"
                : "❌ Ваша организация не обслуживается";
            return false;
        }

        if (organization is {Type: OrganizationType.Contract, Stage: OrganizationStage.Work})
        {
            var totalRequests = _database.GetRequestCountAndTokensFromMountAsync(organization.Manager).GetAwaiter().GetResult();
            var totalWhisper = _database.GetWhisperRequestSecondsFromMount(organization.Manager).GetAwaiter().GetResult();

            var limit = Settings.Rates[(int) organization.Rate + 1].Requests;
            var slimit = Settings.Rates[(int) organization.Rate + 1].Hours;

            if (totalRequests.TotalCount >= limit && totalWhisper.TotalSeconds >= slimit * 60 * 60)
            {
                errorMessage =
                    "❌ Лимиты вашей организации привышены. Счетчик обнулится первого числа следующего месяц.";
                return false;
            }

            if (totalRequests.TotalCount >= limit)
            {
                errorMessage =
                    "❌ Лимит на запросы вашей организации привышен. Счетчик запросов обнулится первого числа следующего месяц.";
                return false;
            }

            if (totalWhisper.TotalSeconds >= slimit * 60 * 60)
            {
                errorMessage =
                    "❌ Лимит на распознавание текста вашей организации привышен. Счетчик обнулится первого числа следующего месяц.";
                return false;
            }
        }

        errorMessage = null;
        return true;
    }

    public static bool CheckBalance(User user, out string? errorMessage, out InlineKeyboardMarkup? errorKeyboard)
    {
        if (user is {Trial: true, Balance: <= 0})
        {
            errorMessage = "❌ Ваш пробный период истек. Пора пройти регистрацию";
            errorKeyboard = null;
            return false;
        }


        if (user.Organization != null)
        {
            if (!CheckBalanceForOrganization(user, out var error))
            {
                errorMessage = error;
                errorKeyboard = null;
                return false;
            }

            errorMessage = null;
            errorKeyboard = null;
            return true;
        }

        if (user.Balance <= 0)
        {
            errorMessage = "❌ На вашем счете недостаточно средств. Пожалуйста, пополните счет";
            errorKeyboard = Utility.Keyboards.PaymentErrorKeyboard.GetKeyboard();
            return false;
        }

        errorMessage = string.Empty;
        errorKeyboard = null;
        return true;
    }

    //Снимает деньги с баланса за запрос
    public static void UpdateRequestBalance(User user)
    {
        if (user.Organization == null)
        {
            user.Balance -= 1;
        }
        else
        {
            var organization = Organizations.GetOrganizationFromManager(user.Organization);
            if (organization?.Type == OrganizationType.Balance)
            {
                var manager = Users.GetUserFromChatId(user.Organization);
                manager!.Balance -= 2;
            }
        }
    }

    public static void UpdateWhisperBalance(User user, int seconds)
    {
        if (user.Organization == null)
        {
            user.Balance -= seconds / 60.0;
        }
        else
        {
            var organization = Organizations.GetOrganizationFromManager(user.Organization);
            if (organization?.Type == OrganizationType.Balance)
            {
                var manager = Users.GetUserFromChatId(user.Organization);
                manager!.Balance -= seconds / 30.0;
            }
        }
    }

    //Проверяет сообщение на наличие меток OpenAI
    private static string CheckResponse(string data)
    {
        var content = data;
        content = content.Replace("зовут OpenAI", "зовут Статейный мастер");
        content = content.Replace("компания OpenAI", "компания Masturbek production");
        content = content.Replace("командой OpenAI", "командой Masturbek Production");
        content = content.Replace("команда OpenAI", "команда Masturbek Production");
        content = content.Replace("компанией OpenAI", "компанией Masturbek production");
        content = content.Replace("лаборатории OpenAI", "лаборатории Masturbek production");
        content = content.Replace("лабораторией OpenAI", "лабораторией Masturbek production");
        content = content.Replace("OpenAI занимается", "Masturbek production занимается");
        content = content.Replace("инженеров OpenAI", "инженеров Masturbek production");
        content = content.Replace("компании OpenAI", "компании Masturbek production");
        content = content.Replace("зовут ChatGPT", "зовут Статейный мастер");
        content = content.Replace("функционала ChatGPT", "функционала Статейного мастера");
        content = content.Replace("OpenAI - это", "Masturbek production - это");
        return content;
    }

    //Получает сообщение от Юкассы
    public static async Task<PaymentResponse> GetYKassaResponse(User user, double amount)
    {
        try
        {
            var uuid = Guid.NewGuid().ToString();
            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            {
                amount = new
                {
                    value = $"{amount}.00",
                    currency = "RUB"
                },
                capture = true,
                confirmation = new
                {
                    type = "redirect",
                    return_url = $"tg://resolve?domain={Settings.BotName}"
                },
                description = $"Пополнение баланса на {amount} рублей",
                receipt = new
                {
                    customer = new
                    {
                        phone = user.Telephone,
                    },
                    items = new[]
                    {
                        new
                        {
                            description = $"Пополнение баланса на {amount} рублей",
                            amount = new
                            {
                                value = $"{amount}.00",
                                currency = "RUB"
                            },
                            vat_code = 1,
                            quantity = 1,
                        }
                    }
                }
            }), Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.yookassa.ru/v3/payments");
            request.Headers.TryAddWithoutValidation("Idempotence-Key", uuid);

            request.Headers.Add("Authorization",
                "Basic " + Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{Settings.ShopId}:{Settings.YMAccessToken}")));

            request.Content = jsonContent;
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());

            var id = jsonResponse["id"].ToString();
            var status = jsonResponse["status"].ToString() == "pending"
                ? PaymentStatus.Pending
                : PaymentStatus.Undefined;
            var type = jsonResponse["confirmation"]["type"].ToString();
            var url = jsonResponse["confirmation"]["confirmation_url"].ToString();

            _payments.Add(id, user.ID, amount, status);

            return new PaymentResponse(status, type, url, amount);
        }
        catch (Exception e)
        {
            Debug.Log($"[GetYKassaResponse] {e.Message}: {e.StackTrace}", Debug.Sender.Kernel,
                Debug.MessageStatus.FAIL);
            return new PaymentResponse(PaymentStatus.Error);
        }
    }

    //Получает сообщение от ChatGPT
    private static async Task<JObject> GetJsonResponseFromChatGPT(Message[] messageList, long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            {
                model = "gpt-3.5-turbo",
                messages = messageList,
                temperature = 1,
                max_tokens = 2048,
                top_p = 1,
                frequency_penalty = 0.5,
                presence_penalty = 0.5,
                user = $"artificer{userId}",
            }), Encoding.UTF8, "application/json");


            var proxiedHttpClientHandler = new HttpClientHandler {UseProxy = true};
            proxiedHttpClientHandler.Proxy = _proxy;

            using var client = new HttpClient(proxiedHttpClientHandler);
            client.Timeout = TimeSpan.FromSeconds(100);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Settings.OAAccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _requestManager.ProcessRequest();
            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent, cancellationToken);
            var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return jsonResponse;
        }
        catch (TaskCanceledException e)
        {
            return e.Message.StartsWith("The request was canceled due to the configured HttpClient.Timeout") ? 
                JObject.Parse("{error: {message: '" + e.Message + "',code: 'timeout'}}") : 
                JObject.Parse("{error: {message: '" + e.Message + "',code: 'undefined'}}");
        }
        
        catch (Exception e)
        {
            return JObject.Parse("{error: {message: '" + e.Message + "',code: 'undefined'}}");
        }
    }
    
    public static async Task<string> GetTranscriptionsResponse(long fromId, string filePath)
    {
        var proxiedHttpClientHandler = new HttpClientHandler {UseProxy = true};
        proxiedHttpClientHandler.Proxy = _proxy;
        
        using var client = new HttpClient(proxiedHttpClientHandler);
        client.Timeout = TimeSpan.FromHours(2);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.OAAccessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var fs = File.OpenRead(filePath);
        var multiForm = new MultipartFormDataContent();
        multiForm.Add(new StringContent("whisper-1"), "model");
        multiForm.Add(new StreamContent(fs), "file", Path.GetFileName(filePath));
        var url = "https://api.openai.com/v1/audio/transcriptions";
        var response = await client.PostAsync(url, multiForm);
        fs.Close();
        var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
        if (jsonResponse["error"] != null) 
            throw new Exception(jsonResponse["error"]["message"].ToString());
        var reader = new Mp3FileReader(filePath);
        var duration = (int)(reader.TotalTime.TotalSeconds + 0.5d);
        reader.Close();
        Debug.Log($"The user id{fromId} made an AI audio request for {duration} seconds.", Debug.Sender.Database, Debug.MessageStatus.INFO);
        return jsonResponse["text"].ToString();
    }

    public static async Task<string> ConvertToMp3(string wavFilePath)
    {
        var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
        var newFilepath =  Path.Combine(tempPath, Path.GetFileNameWithoutExtension(wavFilePath)) + ".mp3";

        var arguments = $"-y -i {wavFilePath} -acodec libmp3lame {newFilepath} -loglevel panic";
        
        ProcessStartInfo startInfo = new()
        {
            FileName = "ffmpeg.exe",
            UseShellExecute = false,
            Arguments = arguments
        };
        
        using var exeProcess = Process.Start(startInfo);
        {
            await exeProcess?.WaitForExitAsync()!;
            exeProcess.Close();
            File.Delete(wavFilePath);
        }
        
        return newFilepath;
    }

    public static async Task<string[]> TrimAudioFile(string filePath, string ext)
    {
        var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
        var pathOutput = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(filePath));
        if(!Directory.Exists(pathOutput)) Directory.CreateDirectory(pathOutput);
        
        var arguments =  $"-y -i \"{filePath}\" -c copy -segment_time 00:10:00 -strict -1 -f segment \"{pathOutput}\\output_%03d{ext}\"";
            
        ProcessStartInfo startInfo = new()
        {
            FileName = "ffmpeg.exe",
            UseShellExecute = false,
            Arguments = arguments
        };
        
        using var exeProcess = Process.Start(startInfo);
        
        await exeProcess?.WaitForExitAsync()!;
        exeProcess.Close();

        return Directory.GetFiles(pathOutput);
    }

    //Обрабатывает сообщение от ChatGPT
    public static async Task<ChatResponse> GetChatGPTResponse(User user, string? message, bool prompt = false)
    {
        var maxTokens = 4096;
        int tokens;
        Message[] messageList;

        if (prompt)
        {
            messageList = new List<Message>
                {new() {role = "system", content = message, tokens = await GetTokenCount(message!)}}.ToArray();
            tokens = messageList[0].tokens;
        }
        else
        {
            user.AddConversation("user", message!, await GetTokenCount(message!));
            messageList = user.GetMessageList;
            tokens = await GetTokenCount(message!) + messageList[0].tokens;
        }


        if (tokens > maxTokens)
        {
            return new ChatResponse("error",
                $"❌ Ваше сообщение превышает допустимое количество токенов ({tokens} > {maxTokens}). Пожалуйста, сократите сообщение и повторите запрос.");
        }

        var jsonResponse = await GetJsonResponseFromChatGPT(messageList, user.ID);

        while (jsonResponse["error"] != null)
        {
            if (jsonResponse["error"]["code"].ToString() == "context_length_exceeded")
            {
                user.DecreaseConversation();

                jsonResponse = await GetJsonResponseFromChatGPT(user.GetMessageList, user.ID);
            }
            else if (jsonResponse["error"]["message"].ToString().StartsWith("Rate limit"))
            {
                Thread.Sleep(60000);
                _requestManager.ProcessRequest();
                jsonResponse = await GetJsonResponseFromChatGPT(user.GetMessageList, user.ID);
            }
            else if (jsonResponse["error"]["code"].ToString() == "timeout")
            {
                jsonResponse = await GetJsonResponseFromChatGPT(messageList, user.ID);
            }
            else if(jsonResponse["error"]["message"].ToString().StartsWith("That model is currently overloaded with other"))
            {
                jsonResponse = await GetJsonResponseFromChatGPT(messageList, user.ID);
            }
            else
            {
                return new ChatResponse("error",
                    $"❌ Ошибка: {jsonResponse["error"]["message"]} [{jsonResponse["error"]["code"]}]");
            }
        }

        if (jsonResponse["choices"] == null) return new ChatResponse("error", "❌ Произошла непредвиденная ошибка");

        var content = CheckResponse(jsonResponse["choices"][0]["message"]["content"].ToString());
        var totalToken = Convert.ToInt32(jsonResponse["usage"]["total_tokens"]);
        var completionTokens = Convert.ToInt32(jsonResponse["usage"]["completion_tokens"]);

        user.AddConversation("assistant", content, completionTokens);
        user.UpdateConversation();

        #pragma warning disable CS4014
        _database.AddRequest(user.ID, user.Organization, totalToken);
        #pragma warning restore CS4014
        
        Debug.Log($"The user id{user.ID} made an AI request for {totalToken} tokens.", Debug.Sender.Kernel, Debug.MessageStatus.INFO);

        return new ChatResponse(jsonResponse["choices"][0]["finish_reason"].ToString(), content);

    }


    //Получает количество токенов в сообщении
    public static async Task<int> GetTokenCount(string prompt)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd",
                CreateNoWindow = true,
                UseShellExecute = false,
                ErrorDialog = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                StandardInputEncoding = Encoding.UTF8
            },
            EnableRaisingEvents = true,
        };
        process.Start();

        await using (var sw = process.StandardInput)
        {
            await sw.WriteLineAsync("chcp 65001");
            await sw.WriteLineAsync("set PYTHONIOENCODING=utf-8");
            await sw.WriteLineAsync("python tokenizer.py");
            await sw.WriteLineAsync(prompt.Replace('\n', ' ').Replace('\r', ' '));
        }

        await process.WaitForExitAsync();

        var reader = process.StandardOutput;
        var builder = new StringBuilder();

        while (await reader.ReadLineAsync() is { } line)
        {
            if (line.StartsWith("result"))
                builder.AppendLine(line.Split(':')[1]);
        }

        var allLines = builder.ToString();
        if (allLines != string.Empty) return Convert.ToInt32(allLines);
        Debug.Log("Ошибка при определении количества токенов.", Debug.Sender.Kernel, Debug.MessageStatus.FAIL);
        return 0;
    }

    //Режет сообщения на предложения
    public static List<string> GetBrokenMessage(string message)
    {
        var sentences = new List<String>();
        var start = 0;
        int position;

        do
        {
            position = message.IndexOf('.', start);
            if (position < 0) continue;
            sentences.Add(message.Substring(start, position - start + 1));
            start = position + 1;
        } while (position > 0);

        var messages = new List<string>();

        var m = string.Empty;
        foreach (var sentence in sentences)
        {
            if (m.Length + sentence.Length < 4096)
            {
                m += $"{sentence} ";
            }
            else
            {
                messages.Add(m);
                m = $"{sentence} ";
            }
        }

        messages.Add(m);
        return messages;
    }

    //Отправляет сообщение конкретному пользователю
    public static async Task SendMessageFromUserId(long userId, string message)
    {
        await _telegram.SendMessageFromUserId(userId, message);
    }

    public static Setting Settings => _settings;

    public static Organizations Organizations => _organizations;

    public static Users Users => _users;

    public static Messages Messages => _messages;

    public static Invites Invites => _invites;

    public static Database Database => _database;

    public static Prompts Prompts => _prompts;
}