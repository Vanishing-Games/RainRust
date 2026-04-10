using Core;
using GameMain.RunTime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor.Tools
{
    public class AudioManagerHelperSheetGenerator : OdinEditorWindow
    {
        private const string SheetOutputPath =
            "Assets/GameMain/Configurations/Audio/audioSheet_audio_manager_helper.asset";

        [MenuItem("Tools/Audio/Generate AudioManagerHelper Sheet")]
        private static void OpenWindow()
        {
            GetWindow<AudioManagerHelperSheetGenerator>("AudioManagerHelper Sheet Generator")
                .Show();
        }

        [Title("生成配置")]
        [LabelText("输出路径")]
        [ReadOnly]
        public string OutputPath = SheetOutputPath;

        [Button(ButtonSizes.Large, Name = "生成 AudioManagerHelper Sheet")]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        private void Generate()
        {
            var sheet = ScriptableObject.CreateInstance<AudioEventSheet>();

            // ─── BGM ────────────────────────────────────────────────────────────────
            sheet.Entries.Add(
                new DefaultAudioEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.BgmPlayDefaultEvent),
                    PlayMode = AudioPlayMode.Managed,
                    Managed = new ManagedConfig
                    {
                        StopEventType = typeof(AudioManagerHelperEvents.BgmStopEvent),
                        StopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT,
                        RestartIfPlaying = false,
                    },
                }
            );

            sheet.Entries.Add(
                new DirectPlayManagedEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.BgmPlayEvent),
                    Managed = new ManagedConfig
                    {
                        StopEventType = typeof(AudioManagerHelperEvents.BgmStopEvent),
                        StopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT,
                        RestartIfPlaying = false,
                    },
                }
            );

            sheet.Entries.Add(
                new SetParameterFromEventEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.BgmSetIntensityEvent),
                    ManagedId = "bgm",
                    ParameterName = "intensity",
                }
            );

            // ─── Ambience ───────────────────────────────────────────────────────────
            sheet.Entries.Add(
                new DefaultAudioEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.AmbiencePlayDefaultEvent),
                    PlayMode = AudioPlayMode.Managed,
                    Managed = new ManagedConfig
                    {
                        StopEventType = typeof(AudioManagerHelperEvents.AmbienceStopEvent),
                        StopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT,
                        RestartIfPlaying = false,
                    },
                }
            );

            sheet.Entries.Add(
                new DirectPlayManagedEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.AmbiencePlayEvent),
                    Managed = new ManagedConfig
                    {
                        StopEventType = typeof(AudioManagerHelperEvents.AmbienceStopEvent),
                        StopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT,
                        RestartIfPlaying = false,
                    },
                }
            );

            sheet.Entries.Add(
                new SetParameterFromEventEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.AmbienceSetIntensityEvent),
                    ManagedId = "ambience",
                    ParameterName = "intensity",
                }
            );

            // ─── UI Sounds ──────────────────────────────────────────────────────────
            AddUiOneShot<AudioManagerHelperEvents.UiConfirmSoundDefaultEvent>(sheet);
            AddUiOneShot<AudioManagerHelperEvents.UiCancelSoundDefaultEvent>(sheet);
            AddUiOneShot<AudioManagerHelperEvents.UiHoverSoundDefaultEvent>(sheet);
            AddUiOneShot<AudioManagerHelperEvents.UiErrorSoundDefaultEvent>(sheet);

            // ─── Stinger ────────────────────────────────────────────────────────────
            sheet.Entries.Add(
                new DefaultAudioEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.StingerPlayDefaultEvent),
                    PlayMode = AudioPlayMode.OneShot,
                }
            );

            // ─── SFX at Position ────────────────────────────────────────────────────
            sheet.Entries.Add(
                new DirectPlay3DOneShotEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.SfxAtPositionEvent),
                }
            );

            // ─── Generic Escape Hatch ────────────────────────────────────────────────
            sheet.Entries.Add(
                new DirectPlayOneShotEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.PlayFmodOneShotEvent),
                }
            );

            sheet.Entries.Add(
                new DirectSetParameterEntry
                {
                    ListenEventType = typeof(AudioManagerHelperEvents.SetManagedParameterEvent),
                }
            );

            AssetDatabase.CreateAsset(sheet, SheetOutputPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.SetDirty(sheet);
            EditorUtility.DisplayDialog(
                "完成",
                $"AudioManagerHelper Sheet 已生成：\n{SheetOutputPath}\n\n请在 Inspector 中为 Default 系列 Entry 指定对应的 FMOD 事件路径。",
                "确定"
            );

            Selection.activeObject = sheet;
        }

        private static void AddUiOneShot<TDefault>(AudioEventSheet sheet)
            where TDefault : IEvent
        {
            sheet.Entries.Add(
                new DefaultAudioEntry
                {
                    ListenEventType = typeof(TDefault),
                    PlayMode = AudioPlayMode.OneShot,
                }
            );
        }
    }
}
