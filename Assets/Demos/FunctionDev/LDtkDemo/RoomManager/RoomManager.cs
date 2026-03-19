using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using LDtkUnity;
using Sirenix.Utilities;
using Unity.Cinemachine;
using UnityEngine;

namespace Demos.LDtkDemo.RoomManager
{
    public class RoomManager : MonoSingletonPersistent<RoomManager>
    {
        private void Start()
        {
            player ??= GameObject.FindGameObjectWithTag("Player")?.transform;
            brain ??= FindFirstObjectByType<CinemachineBrain>() ?? CreateCinemachineBrain();

            RefreshRooms();
            UpdateCurrentRoomByPlayerPosition();
        }

        private CinemachineBrain CreateCinemachineBrain()
        {
            GameObject brainObj = new("CinemachineBrain");
            return brainObj.AddComponent<CinemachineBrain>();
        }

        public void RefreshRooms()
        {
            m_AllRooms = FindObjectsByType<Room>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Select(room =>
                {
                    room.Initialize(player);
                    return room;
                })
                .ToList();
        }

        public void UpdateCurrentRoomByPlayerPosition()
        {
            if (player == null)
                return;

            Vector2 playerPos = player.position;
            foreach (var room in m_AllRooms)
            {
                if (room.WorldRect.Contains(playerPos))
                {
                    currentRoom = room;
                    UpdateRoomVisibility();
                    break;
                }
            }
        }

        public void StartTransition(Portal exitPortal)
        {
            if (m_IsTransitioning)
                return;
            StartCoroutine(TransitionRoutine(exitPortal));
        }

        private IEnumerator TransitionRoutine(Portal exitPortal)
        {
            m_IsTransitioning = true;

            SaveGame();
            Time.timeScale = 0;

            var targetResult = FindPortalByIid(exitPortal.TargetTransitionIid);

            if (!targetResult.IsSuccess)
            {
                Core.Logger.LogError(
                    $"Target portal {exitPortal.TargetTransitionIid} not found!",
                    LogTag.Game
                );
                Time.timeScale = 1;
                m_IsTransitioning = false;
                yield break;
            }

            Portal targetPortal = targetResult.Value;
            Room targetRoom = targetPortal.MyRoom;
            Room oldRoom = currentRoom;
            currentRoom = targetRoom;

            MessageBroker.Global.Publish(
                new RoomTransitionStartEvent(oldRoom, targetRoom, exitPortal, targetPortal)
            );

            UpdateRoomVisibility();

            player.position = targetPortal.GetArrivalPosition();

            oldRoom?.SetActive(false);
            targetRoom.SetActive(true);

            yield return new WaitUntil(() => !brain.IsBlending);
            yield return new WaitForSecondsRealtime(0.1f);

            Time.timeScale = 1;

            MessageBroker.Global.Publish(new RoomTransitionEndEvent(targetRoom));

            m_IsTransitioning = false;
        }

        private void UpdateRoomVisibility()
        {
            if (currentRoom == null)
                return;

            HashSet<Room> activeRooms = new HashSet<Room>();
            activeRooms.Add(currentRoom);

            Portal[] portals = currentRoom.GetComponentsInChildren<Portal>(true);
            foreach (var portal in portals)
            {
                FindPortalByIid(portal.TargetTransitionIid)
                    .Match(
                        targetPortal =>
                        {
                            if (targetPortal.MyRoom != null)
                                activeRooms.Add(targetPortal.MyRoom);
                        },
                        error => { }
                    );
            }

            foreach (var room in m_AllRooms)
            {
                bool shouldBeActive = activeRooms.Contains(room);
                if (room == currentRoom)
                {
                    room.SetActive(true);
                }
                else if (shouldBeActive)
                {
                    room.gameObject.SetActive(true);
                    if (room.virtualCamera != null)
                        room.virtualCamera.Priority = 0;
                }
                else
                {
                    room.gameObject.SetActive(false);
                }
            }
        }

        private Result<Portal, string> FindPortalByIid(string iid)
        {
            var portal = FindObjectsByType<Portal>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .FirstOrDefault(p => p.MyInstanceId == iid);

            return portal != null
                ? Result<Portal, string>.Success(portal)
                : Result<Portal, string>.Failure("Portal not found");
        }

        private void SaveGame()
        {
            Core.Logger.LogInfo("Triggering Save...", LogTag.Game);
        }

        [Header("Setup")]
        public Transform player;
        public CinemachineBrain brain;

        [Header("State")]
        public Room currentRoom;
        private List<Room> m_AllRooms = new List<Room>();
        private bool m_IsTransitioning;
    }
}
