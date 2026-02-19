namespace MaskParameterCorrelatedAlarm
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Enums;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			ScriptParam paramCorrelationAlarmInfo = engine.GetScriptParam(65006);

			if (paramCorrelationAlarmInfo == null)
			{
				return;
			}

			string alarmInfo = paramCorrelationAlarmInfo.Value;
			string[] parts = alarmInfo.Split('|');

			if (parts == null || parts.Length < 10)
			{
				return;
			}

			int dmaID = Tools.ToInt32(parts[1]);
			int elementID = Tools.ToInt32(parts[2]);
			int parameterID = Tools.ToInt32(parts[3]);
			string parameterIdx = parts[4];
			int rootAlarmID = Tools.ToInt32(parts[5]);
			int status = Tools.ToInt32(parts[9]);

			engine.GenerateInformation($"MaskParameter script run for element with ID {elementID}, parameter with ID {parameterID} and parameter Idx {parameterIdx}");

			Element[] elements = engine.FindElements(new ElementFilter() { ElementID = elementID }); // Check if element still exists, will throw if not.
			if (elements.Length != 1)
			{
				engine.ExitFail("RunSafe|Element with ID " + elementID + " not found.");
				return;
			}

			if ((AlarmStatus)status != AlarmStatus.Mask && elements[0].IsActive == true)
			{
				AlarmTreeID alarmTreeID = new AlarmTreeID(dmaID, elementID, rootAlarmID);

				SetAlarmStateMessage sam = new SetAlarmStateMessage(alarmTreeID, AlarmUserStatus.Mask, "Masked in automation script as a result of correlation rule.")
				{
					Info = new SA() { Sa = new string[] { "-1" } }, // Time of masked, -1 == until cleared
				};

				engine.SendSLNetMessage(sam);
			}
		}
	}
}
