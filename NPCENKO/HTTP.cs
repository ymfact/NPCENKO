// Copyright © 2007 - 2017 Ryan Wilson
// Copyright © 2016 RyuaNerin
// Copyright © 2017 ymfact

using System;
using System.Net.Http;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using System.Windows.Threading;

namespace NPCENKO {
    public static class HTTP {

        private static string _baseUrl = "http://translate.google.com/translate_t?hl=&ie=UTF-8&text={0}&sl={1}&tl={2}";

        private static object httpLock = new object();

        public static void Translate( string textToTranslate, Action<string> callback ) {
            Translate( textToTranslate, "auto", "ko", callback );
        }
        public static void Translate( string textToTranslate, string inLang, string outLang, Action<string> callback ) {
            string url = string.Format( _baseUrl, textToTranslate, inLang, outLang );
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation( "User-Agent", "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_3; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.70 Safari/533.4" );
            Dispatcher.CurrentDispatcher.Invoke( async () => {
                try {
                    string result = string.Empty;
                    using( var stream = await httpClient.GetStreamAsync( url ).ConfigureAwait( false ) ) {
                        var doc = new HtmlDocument();
                        doc.Load( stream, Encoding.UTF8 );

                        var translated = doc.DocumentNode.SelectSingleNode( "//span[@id='result_box']" );
                        if( translated != null ) {
                            result = HttpUtility.HtmlDecode( translated.InnerText );
                        }
                    }
                    callback( result );
                } catch( Exception ) {
                    callback( null );
                }
            } );
        }

        private static string questBaseUrl = "http://ffxiv.gamerescape.com/wiki/{0}";

        public static void FindQuest( string text, Action<string> callback ) {
            string url = string.Format( questBaseUrl, text.Replace( ' ', '_' ) );
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation( "User-Agent", "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_3; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.70 Safari/533.4" );

            Dispatcher.CurrentDispatcher.Invoke( async () => {
                try {
                    string result = string.Empty;
                    using( var stream = await httpClient.GetStreamAsync( url ).ConfigureAwait( false ) ) {
                        var doc = new HtmlDocument();
                        doc.Load( stream, Encoding.UTF8 );
                        for( int i = 1; i < 9; i++ ) {
                            string journalXpath = string.Format( "//div[@id='mw-content-text']/table[position()={0}]", i );
                            var journalNode = doc.DocumentNode.SelectSingleNode( journalXpath );
                            if( journalNode == null ) {
                                break;
                            }
                            string titleXpath = string.Format( "//div[@id='mw-content-text']/table[position()={0}]/tr/th/div", i );
                            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode( titleXpath );
                            if( titleNode != null ) {
                                string titleDecode = HttpUtility.HtmlDecode( titleNode.InnerText );
                                if( titleDecode != null && titleDecode.Contains( "Journal" ) ) {
                                    string contentXpath = string.Format( "//div[@id='mw-content-text']/table[position()={0}]/tr[position()=2]", i );
                                    HtmlNode contentNode = doc.DocumentNode.SelectSingleNode( contentXpath );
                                    string contentDecode = HttpUtility.HtmlDecode( contentNode.InnerText );
                                    result = contentDecode;
                                    break;
                                }
                            }
                        }
                    }
                    callback( result );
                } catch( Exception ) {
                    callback( null );
                }
            } );
        }
    }
}
