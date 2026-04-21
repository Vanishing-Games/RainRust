using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameMain.RunTime
{
    public class MainMenuController : MonoBehaviour
    {
        private void OnEnable()
        {
            m_Document = GetComponent<UIDocument>();
            if (m_Document == null)
                return;

            var root = m_Document.rootVisualElement;

            // Screens
            m_MainMenuScreen = root.Q<VisualElement>("main-menu-container");
            m_SaveSlotScreen = root.Q<VisualElement>("save-slot-screen");
            m_SettingsScreen = root.Q<VisualElement>("settings-screen");

            // Main Menu Buttons
            m_StartNewGameButton = root.Q<Button>("start-new-game-button");
            m_ContinueButton = root.Q<Button>("continue-button");
            m_SettingsButton = root.Q<Button>("settings-button");
            m_QuitButton = root.Q<Button>("quit-button");

            m_StartNewGameButton?.RegisterCallback<ClickEvent>(evt => OnStartNewGameClicked());
            m_ContinueButton?.RegisterCallback<ClickEvent>(evt => OnContinueClicked());
            m_SettingsButton?.RegisterCallback<ClickEvent>(evt => ShowScreen(m_SettingsScreen));
            m_QuitButton?.RegisterCallback<ClickEvent>(evt => OnQuitClicked());

            // Save Slot Elements
            m_CreateNewSlotButton = root.Q<Button>("create-new-slot-button");
            m_SlotContainer = root.Q<VisualElement>("slot-container");
            m_BackFromSaveButton = root.Q<Button>("back-to-menu-from-save");

            m_CreateNewSlotButton?.RegisterCallback<ClickEvent>(evt => OnCreateNewSlotClicked());
            m_BackFromSaveButton?.RegisterCallback<ClickEvent>(evt => ShowScreen(m_MainMenuScreen));

            // Rename Popup Elements
            m_RenameOverlay = root.Q<VisualElement>("rename-overlay");
            m_RenameTextField = root.Q<TextField>("rename-textfield");
            m_RenameConfirmButton = root.Q<Button>("rename-confirm-button");
            m_RenameCancelButton = root.Q<Button>("rename-cancel-button");

            m_RenameConfirmButton?.RegisterCallback<ClickEvent>(evt =>
                OnRenameConfirmClicked().Forget()
            );
            m_RenameCancelButton?.RegisterCallback<ClickEvent>(evt =>
                m_RenameOverlay.style.display = DisplayStyle.None
            );

            // Settings Elements
            root.Q<Button>("back-to-menu-from-settings")
                ?.RegisterCallback<ClickEvent>(evt => ShowScreen(m_MainMenuScreen));
        }

        private void ShowScreen(VisualElement screenToShow)
        {
            m_MainMenuScreen.style.display = DisplayStyle.None;
            m_SaveSlotScreen.style.display = DisplayStyle.None;
            m_SettingsScreen.style.display = DisplayStyle.None;

            screenToShow.style.display = DisplayStyle.Flex;

            if (screenToShow == m_SaveSlotScreen)
            {
                RefreshSlotsUI();
            }
        }

        private void RefreshSlotsUI()
        {
            m_SlotContainer.Clear();
            var availableSlots = VgSaveSystem.Instance.AvailableSlots;

            foreach (var meta in availableSlots.OrderByDescending(m => m.LastSavedTime))
            {
                var slotBtn = new Button { name = meta.SlotName };
                slotBtn.AddToClassList("slot-button");

                // Content
                var content = new VisualElement();
                content.AddToClassList("slot-content");

                var nameLabel = new Label(meta.DisplayName ?? meta.SlotName);
                nameLabel.AddToClassList("slot-name-label");

                var infoLabel = new Label(
                    $"Last Saved: {meta.LastSavedTime:yyyy-MM-dd HH:mm} | Time: {TimeSpan.FromSeconds(meta.PlayTimeInSeconds):hh\\:mm\\:ss}"
                );
                infoLabel.AddToClassList("slot-info-label");

                content.Add(nameLabel);
                content.Add(infoLabel);
                slotBtn.Add(content);

                // Actions Area
                var actions = new VisualElement();
                actions.AddToClassList("slot-actions");

                var renameBtn = new Button { text = "Rename" };
                renameBtn.AddToClassList("rename-button");
                renameBtn.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation(); // Prevent slot selection
                    OpenRenamePopup(meta.SlotName, meta.DisplayName);
                });
                actions.Add(renameBtn);
                slotBtn.Add(actions);

                slotBtn.RegisterCallback<ClickEvent>(evt =>
                    ContinueGameOnSlot(meta.SlotName).Forget()
                );
                m_SlotContainer.Add(slotBtn);
            }

            // Adjust visibility based on mode if needed
            if (m_IsNewGameMode)
            {
                m_CreateNewSlotButton.AddToClassList("highlighted-button");
            }
            else
            {
                m_CreateNewSlotButton.RemoveFromClassList("highlighted-button");
            }
        }

        private void OnStartNewGameClicked()
        {
            m_IsNewGameMode = true;
            ShowScreen(m_SaveSlotScreen);
        }

        private void OnContinueClicked()
        {
            m_IsNewGameMode = false;
            ShowScreen(m_SaveSlotScreen);
        }

        private void OnCreateNewSlotClicked()
        {
            string newSlotName = $"slot_{DateTime.Now:yyyyMMdd_HHmmss}";
            CLogger.LogInfo($"Creating new save: {newSlotName}", LogTag.UI);
            VgSaveSystem.Instance.SetCurrentSlot(newSlotName);

            var command = new GameFlowCommands.StartGameCommand("Chapter1", "level0");
            command.Execute().Forget();
        }

        private async UniTaskVoid ContinueGameOnSlot(string slotName)
        {
            CLogger.LogInfo($"Loading save: {slotName}", LogTag.UI);
            bool success = await VgSaveSystem.Instance.LoadSlotAsync(slotName);
            if (success)
            {
                var command = new GameFlowCommands.StartGameCommand("Chapter1", "level0");
                command.Execute().Forget();
            }
        }

        private void OpenRenamePopup(string slotName, string currentName)
        {
            m_SlotToRename = slotName;
            m_RenameTextField.value = currentName ?? slotName;
            m_RenameOverlay.style.display = DisplayStyle.Flex;
        }

        private async UniTaskVoid OnRenameConfirmClicked()
        {
            string newName = m_RenameTextField.value;
            if (!string.IsNullOrEmpty(newName))
            {
                bool success = await VgSaveSystem.Instance.RenameSlotAsync(m_SlotToRename, newName);
                if (success)
                {
                    RefreshSlotsUI();
                }
            }
            m_RenameOverlay.style.display = DisplayStyle.None;
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private UIDocument m_Document;

        // Screens
        private VisualElement m_MainMenuScreen;
        private VisualElement m_SaveSlotScreen;
        private VisualElement m_SettingsScreen;

        // Main Menu Buttons
        private Button m_StartNewGameButton;
        private Button m_ContinueButton;
        private Button m_SettingsButton;
        private Button m_QuitButton;

        // Save Slot Elements
        private Button m_CreateNewSlotButton;
        private VisualElement m_SlotContainer;
        private Button m_BackFromSaveButton;

        // Rename Popup Elements
        private VisualElement m_RenameOverlay;
        private TextField m_RenameTextField;
        private Button m_RenameConfirmButton;
        private Button m_RenameCancelButton;
        private string m_SlotToRename;

        private bool m_IsNewGameMode;
    }
}
