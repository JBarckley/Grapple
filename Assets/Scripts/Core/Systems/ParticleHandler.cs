using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    GameController controller;
    Player player;

    [SerializeField]
    GameObject JumpParticles;

    void Start()
    {
        controller = GetComponent<GameController>();
        player = controller.player;
    }

    public void SpawnJumpParticles(float xOffset = 0, float yOffset = 0)
    {
        Instantiate(JumpParticles, player.transform.position + new Vector3(xOffset, yOffset), Quaternion.identity);
    }
}
