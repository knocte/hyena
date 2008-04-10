//
// MenuButton.cs
//
// Author:
//   Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (c) 2008 Scott Peterson
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using Gtk;
using Gdk;

namespace Hyena.Widgets
{
    public class MenuButton : Container
    {
        private ToggleButton button = new ToggleButton ();
        private HBox box = new HBox ();
        private Alignment alignment;
        private Arrow arrow;
        private Widget button_widget;
        private Menu menu;
        private Widget size_widget;
        
        public MenuButton (Widget buttonWidget, Menu menu, bool showArrow)
        {
            button_widget = buttonWidget;
            this.menu = menu;
            menu.Deactivated += delegate { button.Active = false; };
            
            button.Parent = this;
            button.FocusOnClick = false;
            button.Relief = ReliefStyle.None;
            button.Pressed += delegate { button.Active = true; ShowMenu (); };
            button.Activated += delegate { ShowMenu (); };
            
            box.Parent = this;
            
            if (showArrow) {
                box.PackStart (button_widget, true, true, 0);
                alignment = new Alignment (0f, 0.5f, 0f, 0f);
                arrow = new Arrow (ArrowType.Down, ShadowType.None);
                alignment.Add (arrow);
                box.PackStart (alignment, false, false, 5);
                size_widget = box;
            } else {
                button.Add (button_widget);
                size_widget = button;
            }
            
            ShowAll ();
        }
        
        public Widget ButtonWidget {
            get { return button_widget; }
        }
        
        public Menu Menu {
            get { return menu; }
        }
        
        protected override void OnRealized ()
        {
            WidgetFlags |= WidgetFlags.Realized | WidgetFlags.NoWindow;
            GdkWindow = Parent.GdkWindow;
            base.OnRealized ();
        }
        
        protected override void OnUnrealized ()
        {
            WidgetFlags ^= WidgetFlags.Realized;
            base.OnUnrealized ();
        }
        
        protected override void OnSizeRequested (ref Requisition requisition)
        {
            requisition = size_widget.SizeRequest ();
        }
        
        protected override void OnSizeAllocated (Rectangle allocation)
        {
            box.SizeAllocate (allocation);
            button.SizeAllocate (allocation);
            base.OnSizeAllocated (allocation);
        }
        
        protected override void ForAll (bool include_internals, Callback callback)
        {
            callback (button);
            callback (box);
        }
        
        protected void ShowMenu ()
        {
            menu.Popup (null, null, PositionMenu, 1, Gtk.Global.CurrentEventTime);
        }

        private void PositionMenu (Menu menu, out int x, out int y, out bool push_in) 
        {
            Gtk.Requisition menu_req = menu.SizeRequest ();
            int monitor_num = Screen.GetMonitorAtWindow (GdkWindow);
            Gdk.Rectangle monitor = Screen.GetMonitorGeometry (monitor_num < 0 ? 0 : monitor_num);

            GdkWindow.GetOrigin (out x, out y);
            
            y += Allocation.Y;
            x += Allocation.X + (Direction == TextDirection.Ltr
                ? Math.Max (Allocation.Width - menu_req.Width, 0)
                : - (menu_req.Width - Allocation.Width));
            
            if (y + Allocation.Height + menu_req.Height <= monitor.Y + monitor.Height) {
                y += Allocation.Height;
            } else if (y - menu_req.Height >= monitor.Y) {
                y -= menu_req.Height;
            } else if (monitor.Y + monitor.Height - (y + Allocation.Height) > y) {
                y += Allocation.Height;
            } else {
                y -= menu_req.Height;
            }

            push_in = false;
        }
    }
}
