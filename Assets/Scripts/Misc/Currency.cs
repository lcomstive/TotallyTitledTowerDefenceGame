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
public struct Currency
{
	[SerializeField] private float m_Value;
	[SerializeField] private CurrencyUnit m_Unit;

	private static readonly Dictionary<CurrencyUnit, char> m_UnitChars = new Dictionary<CurrencyUnit, char>()
	{
		{ CurrencyUnit.None,        ' ' },
		{ CurrencyUnit.Billion,     'B' },
		{ CurrencyUnit.Million,     'M' },
		{ CurrencyUnit.Thousand,    'K' },
		{ CurrencyUnit.Trillion,    'T' },
	};

	public float Value
	{
		get => m_Value;
		set
		{
			if (m_Value == value)
				return; // No change

			m_Value = value;
			Validate();
			ValueChanged?.Invoke(this);
		}
	}

	public CurrencyUnit Unit
	{
		get => m_Unit;
		set
		{
			if (m_Unit == value)
				return; // No change

			m_Unit = value;
			ValueChanged?.Invoke(this);
		}
	}

	public Currency(int value = 0) : this()
	{
		m_Value = value;
		m_Unit = CurrencyUnit.None;

		Validate();
	}

	public void Copy(Currency other)
	{
		m_Value = other.Value;
		m_Unit = other.Unit;
		Validate();
	}

	public string DisplayValue() => string.Format(m_Unit == CurrencyUnit.None ? "{0:0}" : "{0:0.0}", m_Value) + m_UnitChars[m_Unit];

	private const CurrencyUnit MaxUnit = CurrencyUnit.Trillion;
	public void Validate()
	{
		// Increment value
		while (m_Value > 1000 && m_Unit < MaxUnit)
		{
			m_Value /= 1000.0f;
			m_Unit++;
		}

		// Decrement value
		while (m_Value < 0 && m_Unit > 0)
		{
			m_Value += 1000.0f;
			m_Unit--;
		}

		// At absolute maximum value
		if (m_Value >= 1000 && m_Unit == MaxUnit)
			m_Value = 999;

		// Don't allow negative values if no currency unit
		if (m_Value < 0 && m_Unit == 0)
			m_Value = 0;
	}

	private Currency Reset(int newValue)
	{
		m_Value = newValue;
		m_Unit = CurrencyUnit.None;
		Validate();

		ValueChanged?.Invoke(this);

		return this;
	}

	public override int GetHashCode() => ((int)this).GetHashCode();
	public override bool Equals(object obj) => obj.GetType().Equals(typeof(Currency)) && (int)(Currency)obj == (int)this;
	public override string ToString() => DisplayValue();

	public delegate void OnValueChanged(Currency newValue);
	public event OnValueChanged ValueChanged;

	#region Operator Overloading
	// Operations
	public static Currency operator +(Currency a, Currency b) => a.Reset((int)a + (int)b);
	public static Currency operator -(Currency a, Currency b) => a.Reset((int)a - (int)b);
	public static Currency operator *(Currency a, Currency b) => a.Reset((int)a * (int)b);
	public static Currency operator /(Currency a, Currency b) => a.Reset((int)a / (int)b);

	// Conversion
	public static implicit operator Currency(int value) => new Currency(value);
	public static explicit operator float(Currency a) => a.m_Value * MathF.Pow(1000, (int)a.m_Unit);

	// Comparisons
	public static bool operator <(Currency a, Currency b) => (int)a < (int)b;
	public static bool operator >(Currency a, Currency b) => (int)a > (int)b;
	public static bool operator <=(Currency a, Currency b) => (int)a <= (int)b;
	public static bool operator >=(Currency a, Currency b) => (int)a >= (int)b;
	public static bool operator ==(Currency a, Currency b) => (int)a == (int)b;
	public static bool operator !=(Currency a, Currency b) => (int)a != (int)b;
	#endregion
}
