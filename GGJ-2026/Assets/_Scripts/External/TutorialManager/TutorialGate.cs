using System.Collections.Generic;
using UnityEngine;

public static class TutorialGate
{
	// When true, world clicks are blocked unless whitelisted
	private static bool blockWorld = false;

	// Whitelist accepts Components, GameObjects, or Transforms
	private static readonly HashSet<Object> worldWhitelist = new();

	public static void BlockWorld()
	{
		blockWorld = true;
		worldWhitelist.Clear();
	}

	public static void AllowWorld()
	{
		blockWorld = false;
		worldWhitelist.Clear();
	}

	// Allow only this world object to be clicked while world is blocked (single)
	public static void AllowOnly(Object allowed)
	{
		blockWorld = true;
		worldWhitelist.Clear();
		AddAllowedInternal(allowed);
	}

	// Replace whitelist with a new set (multiple)
	public static void SetWhitelist(IEnumerable<Object> allowed)
	{
		blockWorld = true;
		worldWhitelist.Clear();
		if (allowed == null)
			return;
		foreach (var o in allowed)
			AddAllowedInternal(o);
	}

	// Returns true if world click is allowed for this object
	public static bool IsWorldAllowed(Object candidate)
	{
		if (!blockWorld)
			return true;
		if (candidate == null)
			return false;

		// Try matching by Component, GameObject, or Transform
		if (worldWhitelist.Contains(candidate))
			return true;

		if (candidate is Component c)
		{
			if (worldWhitelist.Contains(c.transform))
				return true;
			if (worldWhitelist.Contains(c.gameObject))
				return true;
		}
		else if (candidate is GameObject go)
		{
			if (worldWhitelist.Contains(go.transform))
				return true;
		}

		return false;
	}

	private static void AddAllowedInternal(Object allowed)
	{
		if (allowed == null)
			return;
		worldWhitelist.Add(allowed);

		// Also add related object types to make matching robust
		if (allowed is Component c)
		{
			worldWhitelist.Add(c.transform);
			worldWhitelist.Add(c.gameObject);
		}
		else if (allowed is GameObject go)
		{
			worldWhitelist.Add(go.transform);
		}
	}
}
