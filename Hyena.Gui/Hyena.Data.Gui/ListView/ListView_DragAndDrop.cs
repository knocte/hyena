//
// ListView_DragAndDrop.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
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

namespace Hyena.Data.Gui
{    
    public static class ListViewDragDropTarget
    {
        public enum TargetType
        {
            ModelSelection
        }
        
        public static readonly TargetEntry ModelSelection =
            new TargetEntry ("application/x-hyena-data-model-selection", TargetFlags.App,
                (uint)TargetType.ModelSelection);
    }
    
    public partial class ListView<T> : Container
    {
        private static TargetEntry [] drag_drop_dest_entries = new TargetEntry [] {
            ListViewDragDropTarget.ModelSelection
        };
        
        protected static TargetEntry [] DragDropDestEntries {
            get { return drag_drop_dest_entries; }
        }
        
        private bool reorderable = false;
        public bool Reorderable {
            get { return reorderable; }
            set {
                reorderable = value;
                OnDragSourceSet ();
                OnDragDestSet ();
            }
        }
        
        private bool force_drag_source_set = false;
        protected bool ForceDragSourceSet {
            get { return force_drag_source_set; }
            set {
                force_drag_source_set = true;
                OnDragSourceSet ();
            }
        }
        
        private bool force_drag_dest_set = false;
        protected bool ForceDragDestSet {
            get { return force_drag_dest_set; }
            set {
                force_drag_dest_set = true;
                OnDragDestSet ();
            }
        }
        
        protected virtual void OnDragDestSet ()
        {
            if (ForceDragDestSet || Reorderable) {
                Gtk.Drag.DestSet (this, DestDefaults.All, DragDropDestEntries, Gdk.DragAction.Move);
            } else {
                Gtk.Drag.DestUnset (this);
            }
        }
        
        protected virtual void OnDragSourceSet ()
        {
            if (ForceDragSourceSet || Reorderable) {
                Gtk.Drag.SourceSet (this, Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask, 
                    DragDropDestEntries, Gdk.DragAction.Copy | Gdk.DragAction.Move);
            } else {
                Gtk.Drag.SourceUnset (this);
            }
        }
        
        private uint drag_scroll_timeout_id;
        private uint drag_scroll_timeout_duration = 50;
        private double drag_scroll_velocity;
        private double drag_scroll_velocity_max = 100.0;
        private int drag_reorder_row_index = -1;
        private int drag_reorder_motion_y = -1;
        
        private void StopDragScroll ()
        {
            drag_scroll_velocity = 0.0;
            
            if (drag_scroll_timeout_id > 0) {
                GLib.Source.Remove (drag_scroll_timeout_id);
                drag_scroll_timeout_id = 0;
            }
        }
        
        protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
        {
            if (!Reorderable) {
                StopDragScroll ();
                drag_reorder_row_index = -1;
                drag_reorder_motion_y = -1;
                InvalidateList ();
                return false;
            }
            
            drag_reorder_motion_y = y;
            DragReorderUpdateRow ();
            
            double scroll_threshold = Allocation.Height * 0.3;
            
            if (y < scroll_threshold) {
                drag_scroll_velocity = -1.0 + (y / scroll_threshold);
            } else if (y > Allocation.Height - scroll_threshold) {
                drag_scroll_velocity = 1.0 - ((Allocation.Height - y) / scroll_threshold);
            } else {
                StopDragScroll ();
                return true;
            }
            
            if (drag_scroll_timeout_id == 0) {
                drag_scroll_timeout_id = GLib.Timeout.Add (drag_scroll_timeout_duration, OnDragScrollTimeout);
            }
            
            return true;
        }
        
        protected override void OnDragLeave (Gdk.DragContext context, uint time)
        {
            StopDragScroll ();
        }
        
        protected override void OnDragEnd (Gdk.DragContext context)
        {
            StopDragScroll ();
            drag_reorder_row_index = -1;
            drag_reorder_motion_y = -1;
            InvalidateList ();
        }
        
        private bool OnDragScrollTimeout ()
        {
            ScrollTo (vadjustment.Value + (drag_scroll_velocity * drag_scroll_velocity_max));
            DragReorderUpdateRow ();
            return true;
        }
        
        private void DragReorderUpdateRow ()
        {
            int row = GetRowAtY (drag_reorder_motion_y) - 1;
            if (row != drag_reorder_row_index) {
                drag_reorder_row_index = row;
                InvalidateList ();
            }   
        }
    }
}