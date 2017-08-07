// Copyright © 2007 - 2017 Ryan Wilson
// Copyright © 2016 RyuaNerin
// Copyright © 2017 ymfact

//#define CodeViewer

using Sharlayan;
using Sharlayan.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using IniParser;
using IniParser.Model;

namespace NPCENKO {
    public partial class MainWindow : Window {

        private readonly DispatcherTimer scanTimer = new DispatcherTimer();
        private readonly DispatcherTimer printTimer = new DispatcherTimer();
        private readonly Queue<Chat> chatQueue = new Queue<Chat>();

        private readonly Dictionary<string, string> ChatCodes = new Dictionary<string, string>();
        private readonly Dictionary<string, string> EchoCodes = new Dictionary<string, string>();

        public MainWindow() {
            InitializeComponent();

            Application.Current.MainWindow.Topmost = true;

            IniData ini = new FileIniDataParser().ReadFile( "NPCENKO.ini" );
            foreach( KeyData keyData in ini.Sections[ "ChatCodes" ] ) {
                ChatCodes.Add( keyData.KeyName, keyData.Value );
            }
            foreach( KeyData keyData in ini.Sections[ "EchoCodes" ] ) {
                EchoCodes.Add( keyData.KeyName, keyData.Value );
            }
            
            StartSharlayan();

            printTimer.Interval = TimeSpan.FromMilliseconds( 90 );
            printTimer.Tick += PrintTimerElapsed;
            printTimer.Start();

            scanTimer.Interval = TimeSpan.FromMilliseconds( 250 );
            scanTimer.Tick += ScanTimerElapsed;
            scanTimer.Start();
        }

        public void Dispose() {
            scanTimer.Tick -= ScanTimerElapsed;
        }

        private void StartSharlayan() {
            Process[] processes = Process.GetProcessesByName( "ffxiv_dx11" );
            if( processes.Length > 0 ) {
                Process process = processes[ 0 ];
                ProcessModel processModel = new ProcessModel {
                    Process = process,
                    IsWin64 = true
                };
                MemoryHandler.Instance.SetProcess( processModel );
            }
        }

        private int _previousArrayIndex = 0;
        private int _previousOffset = 0;
        private readonly object scanLock = new object();
        private bool isScanning = false;
        private void ScanTimerElapsed( object sender, EventArgs e ) {
            if( isScanning ) {
                return;
            }
            lock( scanLock ) {
                if( isScanning ) {
                    return;
                }
                isScanning = true;

                ChatLogReadResult readResult = null;
                try {
                    readResult = Reader.GetChatLog( _previousArrayIndex, _previousOffset );
                } catch( OverflowException ) {
                    return;
                }

                _previousArrayIndex = readResult.PreviousArrayIndex;
                _previousOffset = readResult.PreviousOffset;

                foreach( var chatLogEntry in readResult.ChatLogEntries ) {
                    if( ChatCodes.ContainsKey( chatLogEntry.Code ) ) {
                        AddChat( new Chat( chatLogEntry.Line, ChatCodes[chatLogEntry.Code] ) );
                        AddTranslatedChat( chatLogEntry.Line, Chat.ReservedColor.AutomaticallyTranslated );
                    } else if( EchoCodes.ContainsKey( chatLogEntry.Code ) ) {
                        AddChat( new Chat( chatLogEntry.Line, EchoCodes[ chatLogEntry.Code ] ) );
                        AddTranslatedChat( chatLogEntry.Line, Chat.ReservedColor.ManuallyTranslated );
                        HTTP.FindQuest( chatLogEntry.Line, ( questText ) => {
                            if( string.IsNullOrWhiteSpace( questText ) ){

                            } else {
                                AddTranslatedChat( questText, Chat.ReservedColor.AutomaticallyTranslated );
                            }
                        } );
                    } else {
#if CodeViewer
                        AddChat( new Chat( chatLogEntry.Code + " " + chatLogEntry.Line, Chat.Type.Debug ) );
#endif
                    }
                }
                isScanning = false;
            };
        }

        private bool isPrinting = false;
        private readonly object chatQueueLock = new object();
        private void PrintTimerElapsed( object sender, EventArgs e ) {
            if( isPrinting ) {
                return;
            }
            lock( chatQueueLock ) {
                if( isPrinting ) {
                    return;
                }
                isPrinting = true;

                if( chatQueue.Count > 0 ) {
                    while( chatQueue.Count > 0 ) {
                        Chat chat = chatQueue.Dequeue();
                        if( string.IsNullOrWhiteSpace( chat.Text ) ) {
                            continue;
                        }
                        TextRange textRange = new TextRange( ctlTextBox.Document.ContentEnd, ctlTextBox.Document.ContentEnd );
                        textRange.Text = chat.Text + "\n";
                        textRange.ApplyPropertyValue( TextElement.ForegroundProperty, chat.Brush );
                    }
                    ScrollToBottom();
                }

                isPrinting = false;
            }
        }

        private void AddChat( Chat chat ) {
            lock( chatQueueLock ) {
                chatQueue.Enqueue( chat );
            }
        }

        public void ScrollToBottom() {
            ctlTextBox.ScrollToEnd();
        }

        private string lastSelection = string.Empty;

        private void TextMouseUp( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
            if( e.ChangedButton != MouseButton.Left ) {
                return;
            }
            string selection = ctlTextBox.Selection.Text;
            if( string.IsNullOrWhiteSpace( selection ) ) {
                return;
            }
            if( selection.Contains( "\n" ) ) {
                return;
            }
            if( selection == lastSelection ) {
                return;
            }
            lastSelection = selection;
            AddTranslatedChat( selection, Chat.ReservedColor.ManuallyTranslated, selection + " => {0}" );
        }

        private void AddTranslatedChat( string text, Chat.ReservedColor chatType) {
            AddTranslatedChat( text, chatType, "{0}", ()=>{ } );
        }

        private void AddTranslatedChat( string text, Chat.ReservedColor chatType, Action callback ) {
            AddTranslatedChat( text, chatType, "{0}", callback );
        }
        private void AddTranslatedChat( string text, Chat.ReservedColor chatType, string format ) {
            AddTranslatedChat( text, chatType, format, ()=> { } );
        }

        private void AddTranslatedChat( string text, Chat.ReservedColor chatType, string format, Action callback ) {
#if !CodeViewer
            HTTP.Translate( text, ( translated ) => {
                if( string.IsNullOrWhiteSpace( translated ) ) {
                    return;
                }
                AddChat( new Chat( string.Format( format, translated ), chatType ) );
                callback();
            } );
#endif
        }
    }
}
