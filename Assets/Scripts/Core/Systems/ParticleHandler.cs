using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    Player player;

    Object JumpParticles;

    void Awake()
    {
        player = GetComponent<Player>();

        JumpParticles = AssetDatabase.LoadAssetAtPath<Object>("Assets/Effects/Particle Systems/Jump/JumpParticleSystem.prefab");
    }

    public void SpawnJumpParticles(float xOffset = 0, float yOffset = 0)
    {
        Instantiate(JumpParticles, player.transform.position + new Vector3(xOffset, yOffset), Quaternion.identity);
    }
}
