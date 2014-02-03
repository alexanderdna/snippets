using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FilterBuilder
{
	public class Program
	{
		enum Condition
		{
			LT,
			LTE,
			GT,
			GTE,
			EQ,
			NEQ,
			AND,
			OR
		}

		abstract class Expression
		{
		}

		class BinExpression : Expression
		{
			public Expression Left;
			public Expression Right;
			public Condition Condition;
		}

		class FieldExpression : Expression
		{
			public FieldInfo Field;
		}

		class IntExpression : Expression
		{
			public int Value;
		}

		public class Record
		{
			public int Alpha;
			public int Beta;
			public int Delta;
		}

		static FieldInfo fldAlpha, fldBeta, fldDelta;

		static Program()
		{
			var rec = typeof(Record);
			fldAlpha = rec.GetField("Alpha");
			fldBeta = rec.GetField("Beta");
			fldDelta = rec.GetField("Delta");
		}

		static void EmitExpression(ILGenerator ilgen, Expression expr)
		{
			BinExpression binexpr;
			FieldExpression fldexpr;
			IntExpression intexpr;

			if (null != (binexpr = expr as BinExpression))
			{
				if (binexpr.Condition == Condition.AND)
				{
					EmitExpression(ilgen, binexpr.Left);
					ilgen.Emit(OpCodes.Ldc_I4_0);
					ilgen.Emit(OpCodes.Ceq);
					ilgen.Emit(OpCodes.Ldc_I4_0);
					ilgen.Emit(OpCodes.Ceq);

					EmitExpression(ilgen, binexpr.Right);
					ilgen.Emit(OpCodes.Ldc_I4_0);
					ilgen.Emit(OpCodes.Ceq);
					ilgen.Emit(OpCodes.Ldc_I4_0);
					ilgen.Emit(OpCodes.Ceq);

					ilgen.Emit(OpCodes.And);
				}
				else if (binexpr.Condition == Condition.OR)
				{
					EmitExpression(ilgen, binexpr.Left);
					ilgen.Emit(OpCodes.Ldc_I4_0);
					ilgen.Emit(OpCodes.Ceq);
					ilgen.Emit(OpCodes.Ldc_I4_0);
					ilgen.Emit(OpCodes.Ceq);

					EmitExpression(ilgen, binexpr.Right);
					ilgen.Emit(OpCodes.Ldc_I4_0);
					ilgen.Emit(OpCodes.Ceq);
					ilgen.Emit(OpCodes.Ldc_I4_0);
					ilgen.Emit(OpCodes.Ceq);

					ilgen.Emit(OpCodes.Or);
				}
				else
				{
					EmitExpression(ilgen, binexpr.Left);
					EmitExpression(ilgen, binexpr.Right);
					switch (binexpr.Condition)
					{
						case Condition.LT:
							ilgen.Emit(OpCodes.Clt);
							break;
						case Condition.LTE:
							ilgen.Emit(OpCodes.Cgt);
							ilgen.Emit(OpCodes.Ldc_I4_0);
							ilgen.Emit(OpCodes.Ceq);
							break;
						case Condition.GT:
							ilgen.Emit(OpCodes.Cgt);
							break;
						case Condition.GTE:
							ilgen.Emit(OpCodes.Clt);
							ilgen.Emit(OpCodes.Ldc_I4_0);
							ilgen.Emit(OpCodes.Ceq);
							break;
						case Condition.EQ:
							ilgen.Emit(OpCodes.Ceq);
							break;
						case Condition.NEQ:
							ilgen.Emit(OpCodes.Ceq);
							ilgen.Emit(OpCodes.Ldc_I4_0);
							ilgen.Emit(OpCodes.Ceq);
							break;
					}
				}
			}
			else if (null != (fldexpr = expr as FieldExpression))
			{
				ilgen.Emit(OpCodes.Ldarg_0);
				ilgen.Emit(OpCodes.Ldfld, fldexpr.Field);
			}
			else if (null != (intexpr = expr as IntExpression))
			{
				ilgen.Emit(OpCodes.Ldc_I4, intexpr.Value);
			}
		}

		static void Main(string[] args)
		{
			Expression filter = new BinExpression
			{
				Left = new BinExpression
				{
					Left = new FieldExpression { Field = fldAlpha },
					Right = new IntExpression { Value = 10 },
					Condition = Condition.LT
				},
				Right = new BinExpression
				{
					Left = new FieldExpression { Field = fldBeta },
					Right = new FieldExpression { Field = fldDelta },
					Condition = Condition.NEQ
				},
				Condition = Condition.OR
			};

			DynamicMethod method = new DynamicMethod("Go", typeof(bool), new Type[] { typeof(Record) });

			ILGenerator ilgen = method.GetILGenerator();
			EmitExpression(ilgen, filter);
			ilgen.Emit(OpCodes.Ret);

			Func<Record, bool> action = method.CreateDelegate(typeof(Func<Record, bool>)) as Func<Record, bool>;
			bool result = action.Invoke(new Record { Alpha = 4, Beta = 5, Delta = 5 });

			Console.WriteLine("Result is {0}", result.ToString());

			Console.ReadLine();
		}
	}
}
