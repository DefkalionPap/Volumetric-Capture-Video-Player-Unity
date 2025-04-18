﻿// Better Streaming Assets, Piotr Gwiazdowski <gwiazdorrr+github at gmail.com>, 2017

using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Android;
using System;
using System.Reflection;

namespace Better.StreamingAssets
{
    [TestFixture("Assets/StreamingAssets", false)]
    [TestFixture("BetterStreamingAssetsTest.apk", true)]
    [TestFixture("BetterStreamingAssetsTest_standalones_standalone-armeabi_v7a.apk", true)]
    [TestFixture("BetterStreamingAssetsTest_splits_base-master.apk", true)]

    public class BetterStreamingAssetsTests
    {
        private const int SizesCount = 2;
        private const int TexturesCount = 2;
        private const int BundlesTypesCount = 3;

        private const string TestDirName = "bsatest";
        private const string TestPath = "Assets/StreamingAssets/" + TestDirName;
        private const string TestResourcesPath = "Assets/Resources/" + TestDirName;
        private const int TestFiles = SizesCount * 2 + SizesCount * 2 * BundlesTypesCount + TexturesCount * BundlesTypesCount + TexturesCount;
        private readonly bool _apkMode;
        private static int[] SizesMB = new int[SizesCount] { 10, 50 };
        private static string[] BundlesLabels = new string[BundlesTypesCount] { "lzma", "lz4", "uncompressed" };
        private static BuildAssetBundleOptions[] BundlesOptions = new BuildAssetBundleOptions[BundlesTypesCount] { BuildAssetBundleOptions.None, BuildAssetBundleOptions.ChunkBasedCompression, BuildAssetBundleOptions.UncompressedAssetBundle };
        
        [MenuItem("Assets/Better Streaming Assets/Make Android Build")]
        public static void CreateAndroidBuild()
        {
            const string TestSceneGuid = "2bef88fd675ce3f4fa61ff5f18aa8242";

            // find test scene
            var path = AssetDatabase.GUIDToAssetPath(TestSceneGuid);
            if (string.IsNullOrEmpty(path))
            {
                throw new System.InvalidOperationException($"Failed to find test scene by guid: {TestSceneGuid}");
            }

            // build
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                scenes = new[] { path },
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                locationPathName = "BetterStreamingAssetsTest.apk",
            });

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException($"Build failed: {EditorJsonUtility.ToJson(report.summary)}");
            }
        }

        [MenuItem("Assets/Better Streaming Assets/Generate Test Data")]
        public static void GenerateTestData()
        {
            if (Directory.Exists(TestPath))
                Directory.Delete(TestPath, true);
            if (Directory.Exists(TestResourcesPath))
                Directory.Delete(TestResourcesPath, true);

            Directory.CreateDirectory(TestPath);
            Directory.CreateDirectory(TestResourcesPath);

            List<string> paths = new List<string>();

            try
            {
                var random = new System.Random(126556343);
                long mb = 1024 * 1024;
                foreach (var size in SizesMB)
                {
                    var p = "Assets/raw_compressable_" + size.ToString("00") + "mb.bytes";
                    paths.Add(p);
                    CreateZeroFile(p, size * mb);
                    p = "Assets/raw_uncompressable_" + size.ToString("00") + "mb.bytes";
                    paths.Add(p);
                    CreateRandomFile(p, size * mb, random);
                }

                {
                    var tex = new Texture2D(2048, 2048, TextureFormat.RGBA32, true);
                    tex.Apply();
                    var bytes = tex.EncodeToPNG();
                    File.WriteAllBytes("Assets/raw_tex_compressible.png", bytes);
                    paths.Add("Assets/raw_tex_compressible.png");
                }

                {
                    var tex = new Texture2D(2048, 2048, TextureFormat.RGBA32, true);

                    var pixels = tex.GetPixels32();

                    byte[] buffer = new byte[4];

                    for (int y = 0; y < tex.height; ++y)
                    {
                        for (int x = 0; x < tex.width; ++x)
                        {
                            random.NextBytes(buffer);
                            pixels[y * tex.width + x] = new Color32(buffer[0], buffer[1], buffer[2], buffer[3]);
                        }
                    }

                    tex.SetPixels32(pixels);
                    tex.Apply();

                    var bytes = tex.EncodeToPNG();
                    File.WriteAllBytes("Assets/raw_tex_uncompressible.png", bytes);
                    paths.Add("Assets/raw_tex_uncompressible.png");
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

                // single bundles

                var tempDirPath = FileUtil.GetUniqueTempPathInProject();
                Directory.CreateDirectory(tempDirPath);

                try
                {
                    for (int i = 0; i < BundlesLabels.Length; ++i)
                    {
                        // now create bundles!
                        var builds = paths.Select(x => new AssetBundleBuild()
                        {
                            assetBundleName = Path.GetFileNameWithoutExtension(x.Replace("raw_", "bundle_")),
                            assetBundleVariant = BundlesLabels[i],
                            assetNames = new[] { x }
                        }).ToArray();

                        BuildPipeline.BuildAssetBundles(tempDirPath, builds, BundlesOptions[i], BuildTarget.Android);
                    }

                    foreach (var file in Directory.GetFiles(tempDirPath).Where(x => Path.GetFileName(x).StartsWith("bundle_") && Path.GetExtension(x) != ".manifest"))
                    {
                        File.Copy(file, Path.Combine(TestResourcesPath, Path.GetFileName(file) + ".bytes"));
                        File.Move(file, Path.Combine(TestPath, Path.GetFileName(file)));
                    }

                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                }
                finally
                {
                    Directory.Delete(tempDirPath, true);
                }

                foreach (var p in paths)
                {
                    var extension = ".bytes";
                    if (Path.GetExtension(p) == ".png")
                        extension = ".png";
                    File.Copy(p, Path.Combine(TestResourcesPath, Path.GetFileName(p) + extension));
                    File.Move(p, Path.Combine(TestPath, Path.GetFileName(p)));
                }
            }
            finally
            {
                foreach (var p in paths)
                {
                    if (File.Exists(p))
                        File.Delete(p);
                }
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            }
        }

        [MenuItem("Assets/Better Streaming Assets/Generate Test Data (Wierd Names)")]
        public static void GenerateWeirdNamesTestData()
        { 
            // now weird names
            var parts = new[]
            {
                    "UCASCII",
                    "lcascii",
                    "UCNONASCII_Ą",
                    "lcnonascii_ą",
                    "UCĄ_NONASCII",
                    "lcą_nonascii",
                    "UCWITH.DOT",
                    "lcwith.dot",
                    "",
                };

            var extensions = new[]
            {
                    ".lc",
                    ".UC",
                    ".Mc",
                    ".lcą",
                    ".UCĄ",
                    ""
                };


            foreach (var folder in parts)
            {
                foreach (var name in parts)
                {
                    if (string.IsNullOrEmpty(name))
                        continue;

                    foreach (var extension in extensions)
                    {
                        var path = Path.Combine("Assets/StreamingAssets/bsanametest", folder, name + extension);
                        if (Directory.Exists(path))
                        {
                            path += name;
                        }
                        CreateZeroFile(path, 1024);
                    }
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }


        [MenuItem("Assets/Better Streaming Assets/Delete Test Data")]
        public static void DeleteTestData()
        {
            if (Directory.Exists(TestPath))
                Directory.Delete(TestPath, true);
            if (Directory.Exists(TestResourcesPath))
                Directory.Delete(TestResourcesPath, true);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }


        [MenuItem("Assets/Better Streaming Assets/Convert AAB to APKS")]
        public static void ConvertAABToAPKS()
        {
#if UNITY_ANDROID
            var gradlePath = UnityEditor.Android.AndroidExternalToolsSettings.gradlePath;
            var jdkRootPath = UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath;
            
#else
            // AndroidExternalToolsSettings can't be safely accessed, as users might not have Android module installed
            var type = Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions", true);
            var gradlePathProperty = type.GetProperty("gradlePath", BindingFlags.Public | BindingFlags.Static) ?? throw new InvalidOperationException("gradlePath property not found");
            var jdkRootPathProperty = type.GetProperty("jdkRootPath", BindingFlags.Public | BindingFlags.Static) ?? throw new InvalidOperationException("jdkRootPath property not found");
            var gradlePath = (string)gradlePathProperty.GetValue(null);
            var jdkRootPath = (string)jdkRootPathProperty.GetValue(null);
#endif
            
            // find bundle tool
            var bundleTool = Directory.GetFiles(Path.Combine(gradlePath, ".."), "bundletool*.jar").Single();
            var paths = Directory.GetFiles(".", "*.aab");

            try
            {
                foreach (var path in paths)
                {
                    if (EditorUtility.DisplayCancelableProgressBar($"AAB->APK", path, Array.IndexOf(paths, path) / (float)paths.Length))
                    {
                        break;
                    }

                    var outPath = $"{path}.apks";
                    FileUtil.DeleteFileOrDirectory(outPath);

                    var processStartInfo = new System.Diagnostics.ProcessStartInfo()
                    {
                        Environment = { { "JAVA_HOME", jdkRootPath } },
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        FileName = Path.Combine(jdkRootPath, "bin", "java.exe"),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Arguments = $"-jar \"{bundleTool}\" build-apks --bundle {path} --output {outPath}"
                    };

                    var bundleToolProcess = System.Diagnostics.Process.Start(processStartInfo);
                    bundleToolProcess.WaitForExit();

                    if (bundleToolProcess.ExitCode != 0)
                    {
                        Debug.LogError($"Exit code {bundleToolProcess.ExitCode} for {path}:\n{bundleToolProcess.StandardOutput.ReadToEnd()}\n{bundleToolProcess.StandardError.ReadToEnd()}");
                    }

                    using (var archive = new System.IO.Compression.ZipArchive(File.OpenRead(outPath)))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                            {
                                var apkPath = $"{Path.GetFileNameWithoutExtension(path)}_{entry.FullName.Replace('/', '_')}";
                                FileUtil.DeleteFileOrDirectory(apkPath);
                                using (var stream = entry.Open())
                                {
                                    using (var fileStream = File.Create(apkPath))
                                    {
                                        stream.CopyTo(fileStream);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public BetterStreamingAssetsTests(string path, bool apkMode)
        {
            if (apkMode && !File.Exists(path))
                Assert.Inconclusive("Build for Android and name output: " + path);

            _apkMode = apkMode;

            BetterStreamingAssets.IsAndroidCompressedStreamingAsset += (path) =>
            {
                return path.Any(x => !IsAscii(x));
            };

            if ( apkMode )
            {
                BetterStreamingAssets.InitializeWithExternalApk(path);
            }
            else
            {
                BetterStreamingAssets.InitializeWithExternalDirectories(".", path);
            }
        }

        [Test]
        public void TestAssetBundleMatchRawData()
        {
            NeedsTestData();

            foreach (var size in SizesMB)
            {
                foreach (var format in new[] { "raw_uncompressable_{0:00}mb.bytes", "raw_uncompressable_{0:00}mb.bytes" })
                {
                    var name = string.Format(format, size);
                    var referenceBytes = BetterStreamingAssets.ReadAllBytes(TestDirName + "/" + name);
                    Assert.AreEqual(size * 1024 * 1024, referenceBytes.Length);

                    foreach (var suffix in BundlesLabels)
                    {
                        var bundleName = Path.GetFileNameWithoutExtension(name).Replace("raw_", "bundle_") + "." + suffix;

                        var bundle = BetterStreamingAssets.LoadAssetBundle(TestDirName + "/" + bundleName);
                        try
                        {
                            var textAsset = (TextAsset)bundle.LoadAllAssets()[0];
                            Assert.AreEqual(Path.GetFileNameWithoutExtension(name), textAsset.name, bundleName);

                            var bytes = textAsset.bytes;
                            Assert.Zero(memcmp(bytes, referenceBytes, bytes.Length), bundleName);
                        }
                        finally
                        {
                            bundle.Unload(true);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestReadAllBytesCompareWithProjectFiles()
        {
            var files = GetRealFiles("/", null, SearchOption.AllDirectories);
            foreach (var f in files)
            {
                if (IsPathExpectedToBeCompressedAnyway(f))
                {
                    continue;
                }
                var a = File.ReadAllBytes("Assets/StreamingAssets/" + f);
                var b = BetterStreamingAssets.ReadAllBytes(f);
                Assert.AreEqual(a.Length, b.Length);
                Assert.Zero(memcmp(a, b, a.Length));
            }

            Assert.Throws<FileNotFoundException>(() => BetterStreamingAssets.ReadAllBytes("FileThatShouldNotExist"));
        }

        //[Test]
        public void ReadAllBytesZeroFile()
        {
            NeedsTestData();

            foreach (var path in BetterStreamingAssets.GetFiles(TestDirName, "raw_compressable*", SearchOption.TopDirectoryOnly))
            {
                var bytes = BetterStreamingAssets.ReadAllBytes(path);
                for (int i = 0; i < bytes.Length; ++i)
                {
                    if (bytes[i] != 0)
                    {
                        Assert.Fail();
                    }
                }
            }
        }

        [Test]
        public void TestOpenReadCompareWithProjectFiles()
        {
            var files = GetRealFiles("/", null, SearchOption.AllDirectories);
            foreach (var f in files)
            {
                if (IsPathExpectedToBeCompressedAnyway(f))
                {
                    continue;
                }
                using (var a = File.OpenRead("Assets/StreamingAssets/" + f))
                using (var b = BetterStreamingAssets.OpenRead(f))
                {
                    Assert.IsTrue(StreamsEqual(a, b), f);
                }
            }

            Assert.Throws<FileNotFoundException>(() => BetterStreamingAssets.OpenRead("FileThatShouldNotExist"));
        }

        [Test]
        public void TestDirectoriesInProjectExistInStreamingAssets()
        {
            var files = GetRealFiles("/", null, SearchOption.AllDirectories, dirs: true);
            foreach (var f in files)
            {
                Assert.IsTrue(BetterStreamingAssets.DirectoryExists(f), f);
            }

            Assert.IsFalse(BetterStreamingAssets.FileExists("DirectoryThatShouldNotExist"));
        }

        [Test]
        public void TestFilesInProjectExistInStreamingAssets()
        {
            var files = GetRealFiles("/", null, SearchOption.AllDirectories);
            foreach (var f in files)
            {
                if (IsPathExpectedToBeCompressedAnyway(f))
                {
                    Assert.IsFalse(BetterStreamingAssets.FileExists(f), f);
                }
                else
                {
                    Assert.IsTrue(BetterStreamingAssets.FileExists(f), f);
                }
            }

            Assert.IsFalse(BetterStreamingAssets.FileExists("FileThatShouldNotExist"));
        }

        private bool IsPathExpectedToBeCompressedAnyway(string path)
        {
            if (_apkMode)
            {
                var partToCheck = Path.GetExtension(path);
                if (partToCheck.Length == 0)
                {
                    partToCheck = path;
                }
                if (partToCheck.Any(x => !IsAscii(x)))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsAscii(char c)
        {
            return c >= 0 && c < 128;
        }

        [TestCase("/", "*.lz4", SearchOption.AllDirectories, SizesCount * 2)]
        [TestCase(".", null, SearchOption.AllDirectories, TestFiles)]
        [TestCase("/", null, SearchOption.AllDirectories, TestFiles)]
        [TestCase("bsatest", null, SearchOption.AllDirectories, 0)]
        [TestCase("bsatest/", null, SearchOption.AllDirectories, 0)]
        [TestCase("bsatest/.", null, SearchOption.AllDirectories, 0)]
        [TestCase("./bsatest/.", null, SearchOption.AllDirectories, 0)]
        [TestCase("/./bsatest/.", null, SearchOption.AllDirectories, 0)]
        [TestCase("/bsatest/.", null, SearchOption.AllDirectories, 0)]
        [TestCase("///////", null, SearchOption.AllDirectories, TestFiles)]
        [TestCase("/.", null, SearchOption.AllDirectories, TestFiles)]
        [TestCase("/./././", null, SearchOption.AllDirectories, TestFiles)]
        [TestCase("bsatest/../bsatest", null, SearchOption.AllDirectories, 0)]
        public void TestFileListInProjectMatchesStreamingAssets(string dir, string pattern, SearchOption opt, int minCount)
        {
            NeedsTestData();
            TestGetFiles(dir, pattern, opt, minCount, int.MaxValue);
        }


        [TestCase(TestDirName, "*.lz4a", SearchOption.TopDirectoryOnly, 0)]
        [TestCase(TestDirName, "*.lz4", SearchOption.TopDirectoryOnly, SizesCount * 2 + TexturesCount)]
        [TestCase(TestDirName, "*.uncompressed", SearchOption.TopDirectoryOnly, SizesCount * 2 + TexturesCount)]
        [TestCase(TestDirName, "*.lzma", SearchOption.TopDirectoryOnly, SizesCount * 2 + TexturesCount)]
        [TestCase(TestDirName, "raw_compressable*", SearchOption.TopDirectoryOnly, SizesCount)]
        [TestCase(TestDirName, "raw_uncompressable*", SearchOption.TopDirectoryOnly, SizesCount)]
        [TestCase(TestDirName, "raw_*", SearchOption.TopDirectoryOnly, SizesCount * 2 + TexturesCount)]
        [TestCase(TestDirName, "gibberish", SearchOption.TopDirectoryOnly, 0)]
        [TestCase(TestDirName, "*", SearchOption.TopDirectoryOnly, TestFiles)]
        public void TestKnownFileListInProjectMatchesStreamingAssets(string dir, string pattern, SearchOption opt, int exactCount)
        {
            NeedsTestData();
            TestGetFiles(dir, pattern, opt, exactCount, exactCount);
        }

        [TestCase("/..")]
        [TestCase("..")]
        [TestCase("C:\\")]
        [TestCase("AAA/../..")]
        [TestCase("/AAA/../..")]
        [TestCase("*")]
        public void TestGetFilesThrow(string dir)
        {
            Assert.Throws<IOException>(() => BetterStreamingAssets.GetFiles(dir, null, SearchOption.TopDirectoryOnly));
        }

        private void TestGetFiles(string dir, string pattern, SearchOption opt, int minCount, int maxCount)
        {
            var files = GetRealFiles(dir, pattern, opt)
                .Where(x => !IsPathExpectedToBeCompressedAnyway(x))
                .ToArray();

            var otherFiles = BetterStreamingAssets.GetFiles(dir, pattern, opt);

            System.Array.Sort(files);
            System.Array.Sort(otherFiles);

            CollectionAssert.AreEqual(files, otherFiles);
        }

        private static string[] GetRealFiles(string nested, string pattern, SearchOption so, bool dirs = false)
        {
            var saDir = Path.GetFullPath("Assets/StreamingAssets/");
            var dir = Path.GetFullPath(saDir + nested);

            if (!Directory.Exists(dir))
                Assert.Inconclusive("Directory " + dir + " doesn't exist");

            List<string> files;

            if (dirs)
            {
                files = Directory.GetDirectories(dir, pattern ?? "*", so)
                    .ToList();
            }
            else
            {
                files = Directory.GetFiles(dir, pattern ?? "*", so)
                    .Where(x => Path.GetExtension(x) != ".meta")
                    .ToList();
            }

            var processedFiles = files.Select(x => x.Replace(saDir, string.Empty).Replace("\\", "/"))
                .ToArray();

            return processedFiles;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        private static bool StreamsEqual(Stream stream1, Stream stream2)
        {
            const int bufferSize = 2048;
            byte[] buffer1 = new byte[bufferSize]; //buffer size
            byte[] buffer2 = new byte[bufferSize];
            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                // You might replace the following with an efficient "memcmp"
                if (memcmp(buffer1, buffer2, count1) != 0)
                    return false;
            }
        }

        private static void CreateRandomFile(string path, long size, System.Random random)
        {
            var data = new byte[size];
            random.NextBytes(data);
            File.WriteAllBytes(path, data);
        }

        private static void CreateZeroFile(string path, long size)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(path, new byte[size]);
        }

        private void NeedsTestData()
        {
            if (!Directory.Exists(TestPath))
                Assert.Inconclusive("Test data not generated. Use \"Assets/Better Streaming Assets/Generate Test Data\" option");
        }
    }
}
