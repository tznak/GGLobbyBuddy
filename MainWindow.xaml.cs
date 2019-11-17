using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Discord;

namespace GGLobbyBuddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MemoryEditor memory_editor;
        private readonly Discord.Discord discord;
        private readonly ActivityManager activity_manager;
        
        private string InviteLink => $"steam://joinlobby/520440/{LobbyId.Text}";

        public MainWindow()
        {
            InitializeComponent();
            
            memory_editor = new MemoryEditor();
            try
            {
                discord = new Discord.Discord(644591817903308819, (ulong) CreateFlags.NoRequireDiscord);
            }
            catch (Exception)
            {
                AutoAccept.Visibility = Visibility.Collapsed;
            }

            if (discord != null)
            {
                activity_manager = discord.GetActivityManager();
                activity_manager.OnActivityJoin += OnActivityJoin;
                activity_manager.OnActivityJoinRequest += OnActivityJoinRequest;
                activity_manager.OnActivityInvite += OnActivityInvite;
            }
            
            StartUpdateLoop();
        }
        
        private void Update()
        {
            StatusBox.Text = memory_editor.Connect() ? "Connected" : "Not Connected";
            
            var lobby_id = memory_editor.GetLobbyId();
            var player_count = memory_editor.GetPlayerCount();
            
            PlayerCurrent.Text = player_count.ToString();
            
            if (lobby_id > 0)
            {
                LobbyId.Text = lobby_id.ToString();
                CopyLinkButton.IsEnabled = true;
            }
            else
            {
                LobbyId.Text = "";
                CopyLinkButton.IsEnabled = false;
            }
        }
        
        private async Task StartUpdateLoop()
        {
            var i = 0;
            while (true)
            {
                Update();

                if (discord != null)
                {
                    discord.RunCallbacks();
                    if (i++ % 5 == 0)
                    {
                        UpdateActivity();
                    }
                }
                
                await Task.Delay(1000);
            }
        }

        private void UpdateActivity()
        {
            var lobby_id = LobbyId.Text;
            if (lobby_id != "")
            {
                activity_manager.UpdateActivity(new Activity
                {
                    State = "In Lobby",
                    Secrets = {Join = lobby_id},
                    Party =
                    {
                        Id = $"_{lobby_id}",
                        Size =
                        {
                            CurrentSize = int.Parse(PlayerCurrent.Text),
                            MaxSize = int.Parse(PlayerMax.Text)
                        }
                    }
                });
            }
            else
            {
                activity_manager.ClearActivity();
            }
        }
        
        private void OnActivityInvite(ActivityActionType type, ref User user, ref Activity activity)
        {
            activity_manager.AcceptInvite(user.Id);
        }

        private void OnActivityJoinRequest(ref User user)
        {
            if (!(AutoAccept.IsChecked ?? false)) return;
            activity_manager.SendRequestReply(user.Id, ActivityJoinRequestReply.Yes);
        }

        private void OnActivityJoin(string secret)
        {
            if (LobbyId.Text == secret) return;
            long.TryParse(secret, out var sanitized);

            Process.Start(new ProcessStartInfo 
            {
                FileName = $"steam://joinlobby/520440/{sanitized}",
                UseShellExecute = true
            }); 
        }


        private void OnLobbySizeTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox text_box)) return;
            if (PlayerMaxSlider == null) return;
            
            int.TryParse(text_box.Text, out var number);
            var clamped = Math.Max(2, Math.Min(8, number));
            
            PlayerMaxSlider.Value = clamped;
            text_box.Text = $"{clamped}";
            text_box.SelectAll();
        }

        private void OnLobbySizeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(sender is Slider slider)) return;
            if (PlayerMax == null) return;
            
            PlayerMax.Text = $"{slider.Value}";
        }

        private void OnCopyInviteLinkClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(InviteLink);
        }
    }
}