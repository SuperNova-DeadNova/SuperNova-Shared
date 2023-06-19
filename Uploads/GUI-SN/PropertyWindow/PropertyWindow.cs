/*
Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/SuperNova)
Dual-licensed under the Educational Community License, Version 2.0 and
the GNU General Public License, Version 3 (the "Licenses"); you may
not use this file except in compliance with the Licenses. You may
obtain a copy of the Licenses at
http://www.opensource.org/licenses/ecl2.php
http://www.gnu.org/licenses/gpl-3.0.html
Unless required by applicable law or agreed to in writing,
software distributed under the Licenses are distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the Licenses for the specific language governing
permissions and limitations under the Licenses.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using SuperNova.Commands;
using SuperNova.Eco;
using SuperNova.Events.GameEvents;
using SuperNova.Games;

namespace SuperNova.Gui 
{
    public partial class PropertyWindow : Form 
    {
        ZombieProperties zsSettings = new ZombieProperties();
        
        public PropertyWindow() {
            InitializeComponent();
            zsSettings.LoadFromServer();
            propsZG.SelectedObject = zsSettings;
        }
        
        public void RunOnUI_Async(Action act) { BeginInvoke(act); }

        void PropertyWindow_Load(object sender, EventArgs e) {
            // try to use same icon as main window
            // must be done in OnLoad, otherwise icon doesn't show on Mono
            GuiUtils.SetIcon(this);
            
            OnMapsChangedEvent.Register(HandleMapsChanged, Priority.Low);
            OnStateChangedEvent.Register(HandleStateChanged, Priority.Low);
            GuiPerms.UpdateRankNames();
            rank_cmbDefault.Items.AddRange(GuiPerms.RankNames);
            rank_cmbOsMap.Items.AddRange(GuiPerms.RankNames);
            blk_cmbMin.Items.AddRange(GuiPerms.RankNames);
            cmd_cmbMin.Items.AddRange(GuiPerms.RankNames);

            //Load server stuff
            LoadProperties();
            LoadRanks();
            try {
                LoadCommands();
                LoadBlocks();
            } catch (Exception ex) {
                Logger.LogError("Error loading commands and blocks", ex);
            }

            LoadGameProps();
        }

        void PropertyWindow_Unload(object sender, EventArgs e) {
            OnMapsChangedEvent.Unregister(HandleMapsChanged);
            OnStateChangedEvent.Unregister(HandleStateChanged);
            Window.hasPropsForm = false;
        }

        void LoadProperties() {
            SrvProperties.Load();
            LoadGeneralProps();
            LoadChatProps();
            LoadRelayProps();
            LoadRelayProps1();
            LoadSqlProps();
            LoadEcoProps();
            LoadMiscProps();
            LoadRankProps();
            LoadSecurityProps();
            zsSettings.LoadFromServer();
        }

        void SaveProperties() {
            try {
                ApplyGeneralProps();
                ApplyChatProps();
                ApplyRelayProps();
                ApplyRelayProps1();
                ApplySqlProps();
                ApplyEcoProps();
                ApplyMiscProps();
                ApplyRankProps();
                ApplySecurityProps();
                
                zsSettings.ApplyToServer();
                SrvProperties.Save();
                Economy.Save();                
            } catch (Exception ex) {
                Logger.LogError(ex);
                Logger.Log(LogType.Warning, "SAVE FAILED! properties/server.properties");
            }
            SaveDiscordProps();
            SaveDiscordProps1();
        }

        void btnSave_Click(object sender, EventArgs e) { SaveChanges(); Dispose(); }
        void btnApply_Click(object sender, EventArgs e) { SaveChanges(); }

        void SaveChanges() {
            SaveProperties();
            SaveRanks();
            SaveCommands();
            SaveBlocks();
            SaveGameProps();
            
            try { ZSGame.Config.Save(); }
            catch { Logger.Log(LogType.Warning, "Error saving Zombie Survival settings!"); }

            SrvProperties.Load(); // loads when saving?
            CommandPerms.Load();
        }

        void btnDiscard_Click(object sender, EventArgs e) { Dispose(); }



#if DEV_BUILD_NOVA
        void GetHelp(string toHelp)
        {
            NovaHelpPlayer p = new NovaHelpPlayer();
            Command.Find("Help").Use(p, toHelp);
            Popup.Message(Colors.StripUsed(p.Messages), "Help for /" + toHelp);
        }
    }
    sealed class NovaHelpPlayer : Player
    {
        public string Messages = "";

        public NovaHelpPlayer() : base("(&5N&do&5v&da)")
        {
            group = Group.NovaRank;
            SuperName = "&5N&do&5v&da";
        }
#else
        void GetHelp(string toHelp) {
            ConsoleHelpPlayer p = new ConsoleHelpPlayer();
            Command.Find("Help").Use(p, toHelp);
            Popup.Message(Colors.StripUsed(p.Messages), "Help for /" + toHelp);
        }
    }
        sealed class ConsoleHelpPlayer : Player {
        public string Messages = "";
            
        public ConsoleHelpPlayer() : base("(console)") {
            group = Group.ConsoleRank;
            SuperName = "Console";
        }
#endif
        public override void Message(byte type, string message) {
            message = Chat.Format(message, this);
            Messages += message + "\r\n";
        }
    }
}