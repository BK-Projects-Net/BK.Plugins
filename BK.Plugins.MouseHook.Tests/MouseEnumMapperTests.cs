using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BK.Plugins.MouseHook;
using BK.Plugins.MouseHook.Common;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;
using NUnit.Framework;
using Shouldly;

namespace BK.Plugins.MouseHook.Tests
{
	[TestFixture]
	public class MouseEnumMapperTests
	{
		[Test]
		public void TestMethod1()
		{
			var mapper = new MouseDictionaryMapper();
			var testCases = GetTestCases();

			foreach (var c in testCases)
			{
				mapper.Map(c.Input).ShouldBe(c.Result);
			}

		}

		private IEnumerable<(MouseHookType Input, MouseInfo Result)> GetTestCases()
		{
			yield return (MouseHookType.WM_LBUTTONDOWN, MouseInfo.LeftButton | MouseInfo.Down);
			yield return (MouseHookType.WM_LBUTTONUP, MouseInfo.LeftButton | MouseInfo.Up);
			yield return (MouseHookType.WM_MBUTTONDOWN, MouseInfo.MiddleButton | MouseInfo.Down);
			yield return (MouseHookType.WM_MBUTTONUP, MouseInfo.MiddleButton | MouseInfo.Up);
			yield return (MouseHookType.WM_RBUTTONDOWN, MouseInfo.RightButton | MouseInfo.Down);
			yield return (MouseHookType.WM_RBUTTONUP, MouseInfo.RightButton | MouseInfo.Up);
			yield return (MouseHookType.WM_MOUSEMOVE, MouseInfo.Move);
			yield return (MouseHookType.WM_MOUSEWHEEL, MouseInfo.Wheel);
		}
	}

}
