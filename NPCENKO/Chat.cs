// Copyright © 2016 RyuaNerin
// Copyright © 2017 ymfact

using System.Windows.Media;

namespace NPCENKO {
    public class Chat {
        public Chat( string text, Type type ) {
            this.text = text;
            if( type == Type.NPC ) {
                brush = greenBrush;
            }
            if( type == Type.Echo ) {
                brush = grayBrush;
            }
            if( type == Type.AutomaticallyTranslated ) {
                brush = whiteBrush;
            }
            if( type == Type.ManuallyTranslated ) {
                brush = redBrush;
            }
            if( type == Type.All ) {
                brush = grayBrush;
            }
        }

        private readonly string text;
        private readonly Brush brush;

        public string Text { get { return text; } }
        public Brush Brush { get { return brush; } }

        private static readonly Brush greenBrush = new SolidColorBrush( (Color) ColorConverter.ConvertFromString( "#acd848" ) );
        private static readonly Brush grayBrush = new SolidColorBrush( (Color) ColorConverter.ConvertFromString( "#cccccc" ) );
        private static readonly Brush whiteBrush = new SolidColorBrush( (Color) ColorConverter.ConvertFromString( "#f7f7f7" ) );
        private static readonly Brush redBrush = new SolidColorBrush( (Color) ColorConverter.ConvertFromString( "#ffa666" ) );

        public enum Type { NPC, Echo, AutomaticallyTranslated, ManuallyTranslated, All };
    }
}
