#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PixelArtAnimationSetup
{
    private class AnimDef
    {
        public string Name;
        public string File;
        public int Frames;
        public float Fps;
        public bool Loop;
    }

    private class CharacterDef
    {
        public string Id;
        public string Folder;
        public string ControllerPath;
        public AnimDef Idle;
        public AnimDef Move;
        public AnimDef Hurt;
        public AnimDef Death;
    }

    [MenuItem("Tools/Dev/PixelArt/Setup Animations")]
    public static void SetupAnimations()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Animations");
        EnsureFolder("Assets/Art/Generated");
        EnsureFolder("Assets/Art/Generated/Animations");

        var characters = new List<CharacterDef>
        {
            new CharacterDef
            {
                Id = "Player_Wizard",
                Folder = "Assets/Art/characters/Characters/Wizard/Wizard",
                ControllerPath = "Assets/Resources/Animations/Player_Wizard.controller",
                Idle = new AnimDef { Name = "Idle", File = "Idle.png", Frames = 6, Fps = 12f, Loop = true },
                Move = new AnimDef { Name = "Run", File = "Run.png", Frames = 8, Fps = 12f, Loop = true },
                Hurt = new AnimDef { Name = "Hurt", File = "Hurt.png", Frames = 6, Fps = 12f, Loop = false },
                Death = new AnimDef { Name = "Death", File = "Death.png", Frames = 7, Fps = 12f, Loop = false }
            },
            new CharacterDef
            {
                Id = "Player_Knight",
                Folder = "Assets/Art/characters/Characters/Knight/Knight",
                ControllerPath = "Assets/Resources/Animations/Player_Knight.controller",
                Idle = new AnimDef { Name = "Idle", File = "Knight-Idle.png", Frames = 6, Fps = 12f, Loop = true },
                Move = new AnimDef { Name = "Run", File = "Knight-Run.png", Frames = 8, Fps = 12f, Loop = true },
                Hurt = new AnimDef { Name = "Hurt", File = "Knight-Hurt.png", Frames = 6, Fps = 12f, Loop = false },
                Death = new AnimDef { Name = "Death", File = "Knight-Death.png", Frames = 7, Fps = 12f, Loop = false }
            },
            new CharacterDef
            {
                Id = "Player_DemonLord",
                Folder = "Assets/Art/characters/Characters/boss/BOSS",
                ControllerPath = "Assets/Resources/Animations/Player_DemonLord.controller",
                Idle = new AnimDef { Name = "Idle", File = "boss-Idle.png", Frames = 7, Fps = 10f, Loop = true },
                Move = new AnimDef { Name = "Fly", File = "boss-Fly.png", Frames = 6, Fps = 12f, Loop = true },
                Hurt = new AnimDef { Name = "Hurt", File = "boss-Hurt.png", Frames = 6, Fps = 12f, Loop = false },
                Death = new AnimDef { Name = "Death", File = "boss-Death.png", Frames = 8, Fps = 12f, Loop = false }
            },
            new CharacterDef
            {
                Id = "Enemy_Slime",
                Folder = "Assets/Art/characters/Characters/Slime/Slime",
                ControllerPath = "Assets/Resources/Animations/Enemy_Slime.controller",
                Idle = new AnimDef { Name = "Idle", File = "Idle.png", Frames = 4, Fps = 8f, Loop = true },
                Move = new AnimDef { Name = "Walk", File = "Walk.png", Frames = 4, Fps = 10f, Loop = true },
                Hurt = null,
                Death = new AnimDef { Name = "Death", File = "Death.png", Frames = 5, Fps = 10f, Loop = false }
            },
            new CharacterDef
            {
                Id = "Enemy_Mushroom",
                Folder = "Assets/Art/characters/Characters/mushroom/mushroom",
                ControllerPath = "Assets/Resources/Animations/Enemy_Mushroom.controller",
                Idle = new AnimDef { Name = "Idle", File = "Idle.png", Frames = 6, Fps = 10f, Loop = true },
                Move = new AnimDef { Name = "Jump", File = "Jump.png", Frames = 7, Fps = 10f, Loop = true },
                Hurt = null,
                Death = new AnimDef { Name = "Death", File = "Death.png", Frames = 5, Fps = 10f, Loop = false }
            },
            new CharacterDef
            {
                Id = "Enemy_Skeleton",
                Folder = "Assets/Art/characters/Characters/Skeleton/Skeleton",
                ControllerPath = "Assets/Resources/Animations/Enemy_Skeleton.controller",
                Idle = new AnimDef { Name = "Idle", File = "Idle.png", Frames = 10, Fps = 12f, Loop = true },
                Move = new AnimDef { Name = "Walk", File = "Walk.png", Frames = 10, Fps = 12f, Loop = true },
                Hurt = new AnimDef { Name = "Hurt", File = "Hurt.png", Frames = 6, Fps = 12f, Loop = false },
                Death = new AnimDef { Name = "Death", File = "Death.png", Frames = 8, Fps = 12f, Loop = false }
            }
        };

        foreach (var def in characters)
        {
            BuildController(def);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void BuildController(CharacterDef def)
    {
        if (def == null)
        {
            return;
        }

        string animDir = $"Assets/Art/Generated/Animations/{def.Id}";
        EnsureFolder(animDir);

        AnimationClip idleClip = def.Idle != null ? CreateClip(def.Folder, def.Idle, animDir) : null;
        AnimationClip moveClip = def.Move != null ? CreateClip(def.Folder, def.Move, animDir) : null;
        AnimationClip hurtClip = def.Hurt != null ? CreateClip(def.Folder, def.Hurt, animDir) : null;
        AnimationClip deathClip = def.Death != null ? CreateClip(def.Folder, def.Death, animDir) : null;

        if (idleClip == null)
        {
            return;
        }

        EnsureFolder(Path.GetDirectoryName(def.ControllerPath));
        var controller = AnimatorController.CreateAnimatorControllerAtPath(def.ControllerPath);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;
        var idle = sm.AddState("Idle");
        idle.motion = idleClip;
        sm.defaultState = idle;

        AnimatorState move = null;
        if (moveClip != null)
        {
            move = sm.AddState("Move");
            move.motion = moveClip;
        }

        AnimatorState hurt = null;
        if (hurtClip != null)
        {
            hurt = sm.AddState("Hurt");
            hurt.motion = hurtClip;
        }

        AnimatorState death = null;
        if (deathClip != null)
        {
            death = sm.AddState("Death");
            death.motion = deathClip;
        }

        if (move != null)
        {
            AddBoolTransition(idle, move, "IsMoving", true);
            AddBoolTransition(move, idle, "IsMoving", false);
        }

        if (death != null)
        {
            var toDeath = sm.AddAnyStateTransition(death);
            toDeath.hasExitTime = false;
            toDeath.duration = 0.05f;
            toDeath.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");
        }

        if (hurt != null)
        {
            var toHurt = sm.AddAnyStateTransition(hurt);
            toHurt.hasExitTime = false;
            toHurt.duration = 0.02f;
            toHurt.AddCondition(AnimatorConditionMode.If, 0f, "Hurt");

            var hurtToIdle = hurt.AddTransition(idle);
            hurtToIdle.hasExitTime = true;
            hurtToIdle.exitTime = 1f;
            hurtToIdle.duration = 0.05f;
        }
    }

    private static void AddBoolTransition(AnimatorState from, AnimatorState to, string param, bool value)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, param);
    }

    private static AnimationClip CreateClip(string folder, AnimDef anim, string outputDir)
    {
        if (anim == null)
        {
            return null;
        }

        string filePath = Path.Combine(folder, anim.File).Replace("\\", "/");
        var sprites = TryLoadFrameSprites(folder, anim) ?? SliceAndLoadSprites(filePath, anim.Frames);
        if (sprites == null || sprites.Length == 0)
        {
            return null;
        }

        string clipPath = Path.Combine(outputDir, $"{anim.Name}.anim").Replace("\\", "/");
        AssetDatabase.DeleteAsset(clipPath);
        var clip = new AnimationClip { frameRate = anim.Fps };
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / anim.Fps,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = anim.Loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private static Sprite[] SliceAndLoadSprites(string path, int frames)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return null;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        if (texture == null || frames <= 0)
        {
            return null;
        }

        int frameWidth = Mathf.Max(1, texture.width / frames);
        int frameHeight = texture.height;

        var metas = new List<SpriteMetaData>();
        string baseName = Path.GetFileNameWithoutExtension(path);
        for (int i = 0; i < frames; i++)
        {
            var meta = new SpriteMetaData
            {
                name = $"{baseName}_{i}",
                rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight),
                alignment = (int)SpriteAlignment.BottomCenter,
                pivot = new Vector2(0.5f, 0f)
            };
            metas.Add(meta);
        }

#pragma warning disable CS0618
        importer.spritesheet = metas.ToArray();
#pragma warning restore CS0618
        importer.SaveAndReimport();

        var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToList();
        sprites.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return sprites.ToArray();
    }

    private static Sprite[] TryLoadFrameSprites(string folder, AnimDef anim)
    {
        if (anim == null || string.IsNullOrEmpty(folder))
        {
            return null;
        }

        var paths = new List<string>();
        for (int i = 0; i < anim.Frames; i++)
        {
            string framePath = Path.Combine(folder, $"{anim.Name}_{i}.png").Replace("\\", "/");
            if (!File.Exists(framePath))
            {
                if (i == 0)
                {
                    return null;
                }
                break;
            }
            paths.Add(framePath);
        }

        if (paths.Count == 0)
        {
            return null;
        }

        var sprites = new List<Sprite>();
        for (int i = 0; i < paths.Count; i++)
        {
            string path = paths[i];
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }

                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        return sprites.Count > 0 ? sprites.ToArray() : null;
    }

    private static void EnsureFolder(string path)
    {
        if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
