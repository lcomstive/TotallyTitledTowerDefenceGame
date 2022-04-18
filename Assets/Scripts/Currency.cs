using System;
using UnityEngine;
using System.Collections.Generic;

public enum CurrencyUnit : byte
{
	None = 0,
	Thousand,
	Million,
	Billion,
	Trillion
}

/// <summary>
/// Represents a currency, with value ranging from 0-9999 with an accompanying enum for units (thousands, millions, etc.).
/// 
/// Negative currency is automatically converted to the lower unit; if there is no unit then the value is set to 0
/// </summary>
[Serializable]
public class Currency
{
	[SerializeField] private int m_Value = 0;
	[SerializeField] private CurrencyUnit m_Unit = CurrencyUnit.None;

	private static readonly Dictionary<CurrencyUnit, char> m_UnitChars = new Dictionary<CurrencyUnit, char>()
	{
		{ CurrencyUnit.None, ' ' },
		{ CurrencyUnit.Billion, 'B' },
		{ CurrencyUnit.Million, 'M' },
		{ CurrencyUnit.Thousand, 'K' },
		{ CurrencyUnit.Trillion, 'T' },
	};

	public int Value
	{
		get => m_Value;
		set
		{
			if(m_Value == value)
				return; // No change

			m_Value = value;
			Validate();
			ValueChanged?.Invoke();
		}
	}

	public CurrencyUnit Unit
	{
		get => m_Unit;
		set
		{
			if(m_Unit == value)
				return; // No change

			m_Unit = value;
			ValueChanged?.Invoke();
		}
	}

	public Currency(int value = 0)
	{
		m_Value = value;
		Validate();
	}

	public string DisplayValue()
	{
		Validate();
		return m_Value.ToString() + m_UnitChars[m_Unit];
	}

	private const CurrencyUnit MaxUnit = CurrencyUnit.Trillion;
	public void Validate()
	{
		while(m_Value > 1000 && m_Unit < MaxUnit)
		{
			m_Value /= 1000;
			m_Unit++;
		}

		if(m_Value < 0 && m_Unit > 0) // Decrement unit
		{
			m_Value += 1000;
			m_Unit--;
		}
		if(m_Value >= 1000 && m_Unit == MaxUnit) // At absolute maximum value
			m_Value = 999;
		if(m_Value < 0 && m_Unit == 0) // Don't allow negative values if no currency unit
			m_Value = 0;
	}

	public override int GetHashCode() => ((int)this).GetHashCode();
	public override bool Equals(object obj) => obj.GetType().Equals(typeof(Currency)) && (int)(Currency)obj == (int)this;

	public delegate void OnValueChanged();
	public event OnValueChanged ValueChanged;

	#region Operator Overloading
	// Operations
	public static Currency operator +(Currency a, Currency b) { a.Value += b.Value; return a; }
	public static Currency operator -(Currency a, Currency b) { a.Value -= b.Value; return a; }
	public static Currency operator *(Currency a, Currency b) { a.Value *= b.Value; return a; }
	public static Currency operator /(Currency a, Currency b) { a.Value /= b.Value; return a; }

	public static Currency operator +(Currency a, int b) { a.Value += b; return a; }
	public static Currency operator -(Currency a, int b) { a.Value -= b; return a; }
	public static Currency operator *(Currency a, int b) { a.Value *= b; return a; }
	public static Currency operator /(Currency a, int b) { a.Value /= b; return a; }

	// Conversion
	public static explicit operator int(Currency a) => a.m_Value * (int)MathF.Pow(1000, (int)a.m_Unit);

	// Comparisons
	public static bool operator <(Currency a, Currency b) => (int)a < (int)b;
	public static bool operator >(Currency a, Currency b) => (int)a > (int)b;
	public static bool operator <=(Currency a, Currency b) => (int)a <= (int)b;
	public static bool operator >=(Currency a, Currency b) => (int)a >= (int)b;
	public static bool operator ==(Currency a, Currency b) => (int)a == (int)b;
	public static bool operator !=(Currency a, Currency b) => (int)a != (int)b;
	#endregion
}
