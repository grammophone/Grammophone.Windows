using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gramma.Windows
{
	/// <summary>
	/// Helpers for binding and validation.
	/// </summary>
	public static class BindingHelper
	{
		/// <summary>
		/// Tests whether all the validators of the given item and all its children
		/// are satisfied.
		/// </summary>
		/// <param name="item">The item to test.</param>
		public static bool AreAllValidated(this DependencyObject item)
		{
			if (item == null) throw new ArgumentNullException("item");

			if ((bool)item.GetValue(Validation.HasErrorProperty)) return false;


			/*
			for (int i = 0, count = VisualTreeHelper.GetChildrenCount(item); i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(item, i);

				if (!AreAllValidated(child)) return false;
			}
			 * */
			
			foreach (object child in LogicalTreeHelper.GetChildren(item))
			{
				DependencyObject dependencyChild = child as DependencyObject;
				if (dependencyChild == null) continue;

				if (!AreAllValidated(dependencyChild)) return false;
			}

			return true;
		}

		/// <summary>
		/// Find the first UIElement that has validation failure
		/// starting from the given item and attempt to show it.
		/// </summary>
		/// <returns>Returns the item found or null.</returns>
		public static UIElement ShowFirstInvalid(this UIElement item)
		{
			UIElement itemFound = item.FindFirstInvalid();

			if (itemFound != null)
			{
				// Attempt to find whether the element is inside a TabItem.
				for (
					DependencyObject parent = LogicalTreeHelper.GetParent(itemFound); 
					parent != null; 
					parent = LogicalTreeHelper.GetParent(parent))
				{
					TabItem tabItem = parent as TabItem;
					if (tabItem != null)
					{
						tabItem.IsSelected = true;
					}
				}

				FrameworkElement element = itemFound as FrameworkElement;
				if (element != null)
				{
					element.BringIntoView();
				}
				itemFound.Focus();
			}

			return itemFound;
		}

		/// <summary>
		/// Find the first UIElement that has validation failure
		/// starting from the given item.
		/// </summary>
		/// <returns>Returns the item found or null.</returns>
		public static UIElement FindFirstInvalid(this UIElement item)
		{
			if (item == null) throw new ArgumentNullException("item");

			if ((bool)item.GetValue(Validation.HasErrorProperty)) return item;

			foreach (object child in LogicalTreeHelper.GetChildren(item))
			{
				UIElement dependencyChild = child as UIElement;
				if (dependencyChild == null) continue;

				UIElement invalidFound = dependencyChild.FindFirstInvalid();
				if (invalidFound != null) return invalidFound;
			}

			return null;
		}
	}
}
