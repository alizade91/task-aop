using Newtonsoft.Json;
using PostSharp.Aspects;
using System;
using System.Text;

namespace WS_Logging
{
	[Serializable]
	public class Logger : OnMethodBoundaryAspect
	{
		public override void OnEntry(MethodExecutionArgs args)
		{
			if (!args.Method.IsConstructor)
			{
				var method = args.Method;
				var methodParametes = method.GetParameters();
				var arguments = args.Arguments;
				var parameters = new StringBuilder();

				for (var i = 0; i < methodParametes.Length; i++)
				{
					var paramNane = methodParametes[i].Name;
					var paramValue = JsonConvert.SerializeObject(arguments[i]);
					parameters.AppendFormat("{0} = {1} ", paramNane, paramValue);
				}

				Log($"{DateTime.Now} call {method.Name} with params: {parameters}");
			}
		}

		public override void OnSuccess(MethodExecutionArgs args)
		{
			if (!args.Method.IsConstructor)
			{
				var method = args.Method;
				var result = JsonConvert.SerializeObject(args.ReturnValue);

				Log($"{DateTime.Now} {method.Name} returns: {result}");
			}
		}

		private void Log(string data)
		{
			Console.WriteLine(data);
		}
	}
}
