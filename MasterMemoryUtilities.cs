using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Chichiche.MasterMemoryUtilities
{
#if UNITY_EDITOR
    using UnityEditor;

    public sealed class MasterMemoryCodeGeneratorWindow : EditorWindow
    {
        static string InputDirectory => EditorPrefs.GetString(nameof( InputDirectory ), Application.dataPath);
        static string MasterMemoryOutputDirectory => EditorPrefs.GetString(nameof( MasterMemoryOutputDirectory ), Application.dataPath);
        static string PrefixClassName => EditorPrefs.GetString(nameof( PrefixClassName ));
        static string UsingNamespace => EditorPrefs.GetString(nameof( UsingNamespace ));
        static bool AddImmutableConstructor => EditorPrefs.GetBool(nameof( AddImmutableConstructor ));
        static bool ReturnNullIfKeyNotFound => EditorPrefs.GetBool(nameof( ReturnNullIfKeyNotFound ));
        static bool ForceOverwrite => EditorPrefs.GetBool(nameof( ForceOverwrite ));
        static bool SkipToGenerateMessagePack => EditorPrefs.GetBool(nameof( SkipToGenerateMessagePack ));
        static string MessagePackOutputDirectory => EditorPrefs.GetString(nameof( MessagePackOutputDirectory ), Application.dataPath);
        static bool GenerateResolverInitializer => EditorPrefs.GetBool(nameof( GenerateResolverInitializer ));
        static bool ResolverInitializerRuntimeInitializeOnLoad => EditorPrefs.GetBool(nameof( ResolverInitializerRuntimeInitializeOnLoad ));

        string _inputDirectory;
        string _masterMemoryOutputDirectory;
        string _prefixClassName;
        string _usingNamespace;
        bool _addImmutableConstructor;
        bool _returnNullIfKeyNotFound;
        bool _forceOverwrite;
        bool _skipToGenerateMessagePack;
        string _messagePackOutputDirectory;
        bool _generateResolverInitializer;
        bool _resolverInitializerRuntimeInitializeOnLoad;

        [MenuItem("Window/MasterMemory/CodeGenerator")]
        public static void ShowWindow()
        {
            GetWindow<MasterMemoryCodeGeneratorWindow>("MasterMemory CodeGen").Show();
        }

        void OnEnable()
        {
            _inputDirectory = InputDirectory;
            _masterMemoryOutputDirectory = MasterMemoryOutputDirectory;
            _prefixClassName = PrefixClassName;
            _usingNamespace = UsingNamespace;
            _addImmutableConstructor = AddImmutableConstructor;
            _returnNullIfKeyNotFound = ReturnNullIfKeyNotFound;
            _forceOverwrite = ForceOverwrite;
            _skipToGenerateMessagePack = SkipToGenerateMessagePack;
            _messagePackOutputDirectory = MessagePackOutputDirectory;
            _generateResolverInitializer = GenerateResolverInitializer;
            _resolverInitializerRuntimeInitializeOnLoad = ResolverInitializerRuntimeInitializeOnLoad;
        }

        void OnGUI()
        {
            var assetsPathContent = new GUIContent("Assets/");
            var assetsPathContentWidth = GUILayout.Width(GUI.skin.label.CalcSize(assetsPathContent).x);
            EditorGUILayout.LabelField("Input directory");
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(assetsPathContent, assetsPathContentWidth);
            _inputDirectory = EditorGUILayout.TextField(_inputDirectory);
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("MasterMemory output directory");
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(assetsPathContent, assetsPathContentWidth);
            _masterMemoryOutputDirectory = EditorGUILayout.TextField(_masterMemoryOutputDirectory);
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Prefix class name (optional)");
            _prefixClassName = EditorGUILayout.TextField(_prefixClassName);
            EditorGUILayout.LabelField("Using namespace (optional)");
            _usingNamespace = EditorGUILayout.TextField(_usingNamespace);
            EditorGUILayout.LabelField("Add immutable constructor");
            _addImmutableConstructor = EditorGUILayout.Toggle(_addImmutableConstructor);
            EditorGUILayout.LabelField("Return null if key not found");
            _returnNullIfKeyNotFound = EditorGUILayout.Toggle(_returnNullIfKeyNotFound);
            EditorGUILayout.LabelField("Force overwrite");
            _forceOverwrite = EditorGUILayout.Toggle(_forceOverwrite);
            EditorGUILayout.LabelField("Skip to generate MessagePack");
            _skipToGenerateMessagePack = EditorGUILayout.Toggle(_skipToGenerateMessagePack);
            EditorGUILayout.LabelField("MessagePack output directory");
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(assetsPathContent, assetsPathContentWidth);
            _messagePackOutputDirectory = EditorGUILayout.TextField(_messagePackOutputDirectory);
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Generate message pack ResolverInitializer script");
            _generateResolverInitializer = EditorGUILayout.Toggle(_generateResolverInitializer);
            EditorGUILayout.LabelField("ResolverInitializer runtime initialize on load");
            _resolverInitializerRuntimeInitializeOnLoad = EditorGUILayout.Toggle(_resolverInitializerRuntimeInitializeOnLoad);

            if (GUILayout.Button("Generate"))
            {
                EditorPrefs.SetString(nameof( InputDirectory ), _inputDirectory);
                EditorPrefs.SetString(nameof( MasterMemoryOutputDirectory ), _masterMemoryOutputDirectory);
                EditorPrefs.SetString(nameof( PrefixClassName ), _prefixClassName);
                EditorPrefs.SetString(nameof( UsingNamespace ), _usingNamespace);
                EditorPrefs.SetBool(nameof( AddImmutableConstructor ), _addImmutableConstructor);
                EditorPrefs.SetBool(nameof( ReturnNullIfKeyNotFound ), _returnNullIfKeyNotFound);
                EditorPrefs.SetBool(nameof( ForceOverwrite ), _forceOverwrite);
                EditorPrefs.SetBool(nameof( SkipToGenerateMessagePack ), _skipToGenerateMessagePack);
                EditorPrefs.SetString(nameof( MessagePackOutputDirectory ), _messagePackOutputDirectory);
                EditorPrefs.SetBool(nameof( GenerateResolverInitializer ), _generateResolverInitializer);
                EditorPrefs.SetBool(nameof( ResolverInitializerRuntimeInitializeOnLoad ), _resolverInitializerRuntimeInitializeOnLoad);
                Generate();
            }
        }

        static void Generate()
        {
            if (! MmGen()) return;
            if (! Mpc()) return;
            GenerateResolverInitializerIfNeeded();
            AssetDatabase.Refresh();
        }

        static bool MmGen()
        {
            using var process = new Process();
            var argumentsBuilder = new StringBuilder();
            argumentsBuilder.Append(" -inputDirectory ").Append(Path.Combine(Application.dataPath, InputDirectory));
            argumentsBuilder.Append(" -outputDirectory ").Append(Path.Combine(Application.dataPath, MasterMemoryOutputDirectory));
            if (! string.IsNullOrEmpty(PrefixClassName)) argumentsBuilder.Append(" -prefixClassName ").Append(PrefixClassName);
            if (! string.IsNullOrEmpty(UsingNamespace)) argumentsBuilder.Append(" -usingNamespace ").Append(UsingNamespace);
            if (AddImmutableConstructor) argumentsBuilder.Append(" --addImmutableConstructor ");
            if (ReturnNullIfKeyNotFound) argumentsBuilder.Append(" --returnNullIfKeyNotFound ");
            if (ForceOverwrite) argumentsBuilder.Append(" --forceOverwrite ");
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet-mmgen",
                Arguments = argumentsBuilder.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            process.Start();
            process.WaitForExit();
            Debug.Log(process.StandardOutput.ReadToEnd());
            return process.ExitCode == 0;
        }

        static bool Mpc()
        {
            if (SkipToGenerateMessagePack) return true;
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "mpc",
                Arguments = $"-input {Path.Combine(Application.dataPath, InputDirectory)} -output {Path.Combine(Application.dataPath, MessagePackOutputDirectory)}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            process.Start();
            process.WaitForExit();
            Debug.Log(process.StandardOutput.ReadToEnd());
            return process.ExitCode == 0;
        }

        static void GenerateResolverInitializerIfNeeded()
        {
            if (SkipToGenerateMessagePack) return;
            if (! GenerateResolverInitializer) return;
            var resolverInitializerFilePath = Path.Combine(Application.dataPath, MessagePackOutputDirectory, "MessagePackResolverInitializer.cs");
            if (File.Exists(resolverInitializerFilePath) && ! ForceOverwrite) return;
            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("using MessagePack;");
            scriptBuilder.AppendLine("using MessagePack.Resolvers;");
            if (ResolverInitializerRuntimeInitializeOnLoad) scriptBuilder.AppendLine("using UnityEngine;");
            if (! string.IsNullOrEmpty(UsingNamespace)) scriptBuilder.Append("namespace ").Append(UsingNamespace).Append('{').AppendLine();
            scriptBuilder.AppendLine("public static class MessagePackResolverInitializer {");
            scriptBuilder.AppendLine("static IFormatterResolver _resolver;");
            if (ResolverInitializerRuntimeInitializeOnLoad) scriptBuilder.AppendLine("[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]");
            scriptBuilder.AppendLine("public static void SetupMessagePackResolver() {");
            scriptBuilder.AppendLine("_resolver ??= CompositeResolver.Create(MasterMemoryResolver.Instance, GeneratedResolver.Instance, StandardResolver.Instance);");
            scriptBuilder.AppendLine("MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(_resolver);");
            scriptBuilder.Append('}');
            scriptBuilder.Append('}');
            if (! string.IsNullOrEmpty(UsingNamespace)) scriptBuilder.Append('}');
            File.WriteAllText(resolverInitializerFilePath, scriptBuilder.ToString());
        }
    }
#endif
}