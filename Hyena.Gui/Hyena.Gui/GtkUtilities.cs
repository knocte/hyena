//
// GtkUtilities.cs
//
// Authors:
//   Aaron Bockover <abockover@novell.com>
//   Andrés G. Aragoneses <knocte@gmail.com>
//
// Copyright 2007-2010 Novell, Inc.
// Copyright 2013 Andrés G. Aragoneses
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Gtk;

namespace Hyena.Gui
{
    public delegate void WidgetAction<T> (T widget) where T : class;

    public static class GtkUtilities
    {
        private static Gdk.ModifierType [] important_modifiers = new Gdk.ModifierType [] {
            Gdk.ModifierType.ControlMask,
            Gdk.ModifierType.ShiftMask
        };

        public static bool NoImportantModifiersAreSet ()
        {
            return NoImportantModifiersAreSet (important_modifiers);
        }

        public static bool NoImportantModifiersAreSet (params Gdk.ModifierType [] modifiers)
        {
            Gdk.ModifierType state;

            if (Global.CurrentEvent is Gdk.EventKey) {
                state = ((Gdk.EventKey)Global.CurrentEvent).State;
            } else if (Global.CurrentEvent is Gdk.EventButton) {
                state = ((Gdk.EventButton)Global.CurrentEvent).State;
            } else {
                return false;
            }

            foreach (Gdk.ModifierType modifier in modifiers) {
                if ((state & modifier) == modifier) {
                    return false;
                }
            }

            return true;
        }

        public static FileFilter GetFileFilter (string name, System.Collections.Generic.IEnumerable<string> extensions)
        {
            FileFilter filter = new FileFilter ();
            filter.Name = name;
            foreach (string extension in extensions) {
                filter.AddPattern (String.Format ("*.{0}", extension.ToLower ()));
                filter.AddPattern (String.Format ("*.{0}", extension.ToUpper ()));
            }
            return filter;
        }

        public static void SetChooserShortcuts (Gtk.FileChooserDialog chooser, params string [] shortcuts)
        {
            foreach (string shortcut in shortcuts) {
                if (shortcut != null) {
                    try {
                        chooser.AddShortcutFolder (shortcut);
                    } catch {}
                }
            }
        }

        public static Gdk.RGBA ColorBlend (Gdk.RGBA a, Gdk.RGBA b)
        {
            // at some point, might be nice to allow any blend?
            double blend = 0.5;

            if (blend < 0.0 || blend > 1.0) {
                throw new ApplicationException ("blend < 0.0 || blend > 1.0");
            }

            double blendRatio = 1.0 - blend;

            double mR = a.Red + b.Red;
            double mG = a.Green + b.Green;
            double mB = a.Blue + b.Blue;

            double blR = mR * blendRatio;
            double blG = mG * blendRatio;
            double blB = mB * blendRatio;

            Gdk.RGBA color = new Gdk.RGBA ();
            color.Red = blR;
            color.Green = blG;
            color.Blue = blB;
            return color;
        }

        public static T StyleGetProperty<T> (Widget widget, string property, T default_value)
        {
            object result = null;
            try {
                result = widget.StyleGetProperty (property);
            } catch {}
            return result != null && result.GetType () == typeof (T) ? (T)result : default_value;
        }

        public static void ForeachWidget<T> (Container container, WidgetAction<T> action) where T : class
        {
            if (container == null) {
                return;
            }

            foreach (Widget child in container.Children) {
                T widget = child as T;
                if (widget != null) {
                    action (widget);
                } else {
                    Container child_container = child as Container;
                    if (child_container != null) {
                        ForeachWidget<T> (child_container, action);
                    }
                }
            }
        }

        internal static string Dump (this Gtk.Adjustment alig) {
            if (alig == null) {
                return "<null>";
            }
            return String.Format("Value:{0},PageSize{1},PageIncrement:{2},StepIncrement:{3},Lower:{4},Upper:{5}",
                                 alig.Value, alig.PageSize, alig.PageIncrement, alig.StepIncrement, alig.Lower, alig.Upper);
        }

        [Obsolete ("Use Gtk.Global.ShowUri() from gtk# 3.x")]
        public static bool ShowUri (string uri)
        {
            return ShowUri (null, uri);
        }

        [Obsolete ("Use Gtk.Global.ShowUri() from gtk# 3.x")]
        public static bool ShowUri (Gdk.Screen screen, string uri)
        {
            return ShowUri (screen, uri, Gtk.Global.CurrentEventTime);
        }

        [Obsolete ("Use Gtk.Global.ShowUri() from gtk# 3.x")]
        public static bool ShowUri (Gdk.Screen screen, string uri, uint timestamp)
        {
            return Gtk.Global.ShowUri (screen, uri, timestamp);
        }
    }
}
