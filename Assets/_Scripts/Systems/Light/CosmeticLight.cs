using UnityEngine;

/// <summary>
/// Маркер для источников света, которые используются только визуально.
/// LightSystem исключает такие источники из кэша спотлайтов,
/// поэтому они не влияют на PlayerLightSensor, AgentLightSensor, MagicalMirror и т.п.
/// </summary>
[DisallowMultipleComponent]
public class CosmeticLight : MonoBehaviour { }
