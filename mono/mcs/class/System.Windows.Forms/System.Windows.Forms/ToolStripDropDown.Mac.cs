﻿using System.ComponentModel;
using System.Drawing;
#if XAMARINMAC
using AppKit;
using Foundation;
#else
using MonoMac.AppKit;
using MonoMac.Foundation;
#endif

namespace System.Windows.Forms
{
	public partial class ToolStripDropDown
	{
		NSMenu currentMenu;

		private void CancelGrab()
		{
			IntPtr grabHandle;
			bool grabConfined;
			Drawing.Rectangle grabArea;
			XplatUI.GrabInfo(out grabHandle, out grabConfined, out grabArea);
			if (grabHandle != IntPtr.Zero)
				XplatUI.SendMessage(grabHandle, Msg.WM_CANCELMODE, IntPtr.Zero, IntPtr.Zero);
		}

		internal NSMenu ToNSMenu()
		{
			var menu = new NSMenu();
			menu.Delegate = new MonoMenuDelegate(this, menu);
			menu.AutoEnablesItems = false;
			return menu;
		}

		// Finds an item that should appear at given position. Passing this item to PopUpMenu() causes adjusting the final menu position.
		// It's not perfect, because real items are inserted and set later (in MenuWillOpen callback).
		NSMenuItem ItemForPosition(Screen screen, Point position)
		{
			is_visible = true;
			int menuHeight = 0;
			int itemHeight = (int)NSApplication.SharedApplication.Menu.MenuBarHeight - 4; // FIXME: Is there a better way to determine menu item height?
			foreach (ToolStripItem tsi in Items)
			{
				currentMenu.AddItem(tsi.ToNSMenuItem());
				//if (item.Visible) // This condition dsoes not work well, because some items get hidden later
				menuHeight += itemHeight;
			}
			is_visible = false;

			var overlap = position.Y + menuHeight - screen.Bounds.Bottom;
			if (overlap > 0)
			{
				int index = (int)Math.Ceiling((double)overlap / (double)itemHeight);
				if (index > 0 && index < Items.Count)
					return currentMenu.ItemAt(index);
			}
			return null;
		}

		[Register("__xmonomac_internal_ActionDispatcher")]
		internal class ActionDispatcher : NSObject
		{
			const string skey = "__xmonomac_internal_ActionDispatcher_activated:";
			const string dkey = "__xmonomac_internal_ActionDispatcher_doubleActivated:";
			public static ObjCRuntime.Selector Action = new ObjCRuntime.Selector(skey);
			public static ObjCRuntime.Selector DoubleAction = new ObjCRuntime.Selector(dkey);
			public EventHandler Activated;
			public EventHandler DoubleActivated;
			//public Func<NSMenuItem, bool> ValidateMenuItemFunc;

			[Preserve, Export(skey)]
			public void OnActivated(NSObject sender)
			{
				EventHandler handler = Activated;
				if (handler != null)
					handler(sender, EventArgs.Empty);
			}

			[Preserve, Export(dkey)]
			public void OnActivated2(NSObject sender)
			{
				EventHandler handler = DoubleActivated;
				if (handler != null)
					handler(sender, EventArgs.Empty);
			}

			public ActionDispatcher(EventHandler handler)
			{
				IsDirectBinding = false;
				Activated = handler;
			}

			public ActionDispatcher()
			{
				IsDirectBinding = false;
			}

			public static NSObject SetupAction(NSObject target, EventHandler handler)
			{
				ActionDispatcher ctarget = target as ActionDispatcher;
				if (ctarget == null)
				{
					ctarget = new ActionDispatcher();
				}
				ctarget.Activated += handler;
				return ctarget;
			}

			public static void RemoveAction(NSObject target, EventHandler handler)
			{
				ActionDispatcher ctarget = target as ActionDispatcher;
				if (ctarget == null)
					return;
				ctarget.Activated -= handler;
			}

			public static NSObject SetupDoubleAction(NSObject target, EventHandler doubleHandler)
			{
				ActionDispatcher ctarget = target as ActionDispatcher;
				if (ctarget == null)
				{
					ctarget = new ActionDispatcher();
				}
				ctarget.DoubleActivated += doubleHandler;
				return ctarget;
			}

			public static void RemoveDoubleAction(NSObject target, EventHandler doubleHandler)
			{
				ActionDispatcher ctarget = target as ActionDispatcher;
				if (ctarget == null)
					return;
				ctarget.DoubleActivated -= doubleHandler;
			}

			/*public bool ValidateMenuItem(NSMenuItem menuItem)
			{
				if (ValidateMenuItemFunc != null)
					return ValidateMenuItemFunc(menuItem);

				return true;
			}*/

			public virtual Boolean WorksWhenModal
			{
				[Export("worksWhenModal")]
				get { return true; }
			}
		}

		class MonoMenuDelegate : NSMenuDelegate
		{
			ToolStripDropDown owner;
			NSMenu menu;

			public MonoMenuDelegate(ToolStripDropDown owner, NSMenu menu)
			{
				this.owner = owner;
				this.menu = menu;
			}

			public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item)
			{
				//base.MenuWillHighlightItem(menu, item);
			}

			public override void MenuWillOpen(NSMenu menu)
			{
				if (owner.currentMenu != menu)
				{
					var cancelEventArgs = new CancelEventArgs(); 
					if (owner.owner_item != null && owner.owner_item is ToolStripDropDownItem)
					{
						ToolStripDropDownItem dropdown_owner = (ToolStripDropDownItem)owner.owner_item;
						dropdown_owner.OnDropDownShow(cancelEventArgs);
					}
					if (!cancelEventArgs.Cancel)
						owner.OnOpening(cancelEventArgs);
					if (cancelEventArgs.Cancel)
					{
						menu.CancelTrackingWithoutAnimation();
						return;
					}
				}

				owner.is_visible = true;
				owner.currentMenu = menu;
				owner.OnVisibleChanged(EventArgs.Empty);

				// Send WM_CANCELMODE to cancel any grabs
				owner.CancelGrab();

				// Convert all the menu items to NSMenuItems (w/ embedded views if necessary)
				menu.RemoveAllItems();
				foreach (ToolStripItem item in owner.Items)
				{
					var menuItem = item.ToNSMenuItem();
					var actionObj = new ActionDispatcher((sender, e) =>
						{
							owner.OnItemClicked(new ToolStripItemClickedEventArgs(item)); item.PerformClick();
						});
					menuItem.Target = actionObj;
					menuItem.Action = ActionDispatcher.Action;
					/*menuItem.Activated += (sender, e) => {
						owner.OnItemClicked(new ToolStripItemClickedEventArgs(item)); item.PerformClick();
					};*/
					menu.AddItem(menuItem);
				}

				owner.OnOpened(EventArgs.Empty);
			}

			public override void MenuDidClose(NSMenu menu)
			{
				// Send WM_CANCELMODE to cancel any grabs
				menu.RemoveAllItems();
				owner.CancelGrab();
				owner.currentMenu = null;
				owner.Close();
				owner.is_visible = false;
				owner.OnVisibleChanged(EventArgs.Empty);
			}	
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (currentMenu == null)
				base.OnPaintBackground(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (currentMenu == null)
				base.OnPaint(e);
		}
	}
}