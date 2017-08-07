// Copyright © 2016 RyuaNerin
// Copyright © 2017 ymfact

using System.Collections.Generic;
using System.Windows.Media;

namespace NPCENKO {
    public class Chat {
        public Chat( string text, ReservedColor type ) {
            this.text = text;
            if( type == ReservedColor.AutomaticallyTranslated ) {
                brush = GetBrush( "#f7f7f7" );
            }
            if( type == ReservedColor.ManuallyTranslated ) {
                brush = GetBrush( "#ffa666" );
            }
            if( type == ReservedColor.Debug ) {
                brush = GetBrush( "#cccccc" );
            }
        }

        public Chat( string text, string color ) {
            this.text = text;
            this.brush = GetBrush( color );
        }

        private static Brush GetBrush( string color ) {
            Brush brush;
            if( brushes.ContainsKey( color ) ) {
                brush = brushes[ color ];
            }else {
                brush = new SolidColorBrush( (Color) ColorConverter.ConvertFromString( color ) );
                brush.Freeze();
                brushes.Add( color, brush );
            }
            return brush;
        }

        private readonly string text;
        private readonly Brush brush;

        public string Text { get { return text; } }
        public Brush Brush { get { return brush; } }

        private static readonly Dictionary<string, Brush> brushes = new Dictionary<string, Brush>();

        public enum ReservedColor { AutomaticallyTranslated, ManuallyTranslated, Debug };
    }
}
