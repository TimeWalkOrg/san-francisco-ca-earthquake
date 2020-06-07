using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace WireBuilder
{
    public class WireBuilderGUI : Editor
    {
        static WireBuilderGUI()
        {
            ErrorIcon = CreateIcon(ErrorIconData);
            GroupIcon = CreateIcon(GroupIconData);
            ConnectorIcon = CreateIcon(ConnectorIconData);
            WireIcon = CreateIcon(WireIconData);
            DragIcon = CreateIcon(DragIconData);
            DuplicateIcon = CreateIcon(DuplicateIconData);
            DeleteIcon = CreateIcon(DeleteIconData);
            ConnectIcon = CreateIcon(ConnectIconData);
        }
        private static Color originalColor;
        const int iconWidth = 16;
        const int iconHeight = 16;

        private const string ErrorIconData = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAACFUlEQVQ4EW2TvWsUURTF78xsjFmQVTZFiE0EkbQ2LigisRGtRPCjTGGI9iYIKlhoISL4T4iNBERLhZBStElvYbFRiIGY/ZrZnZnn78y+WTeLDw73vnfPPffOffMC55zt7Pyyev2ERVHFOp2u1WrH5pOkf2N6+siSmYVwtvI838D/EQSBtVptm5k5annurMLh5Jrl4PnUVGVZ4koA14MgPOdc/oBY81CCSM3mT4vj2AaD9HaWZc7jdZqm1+jkKnhJcgZXsZX9/T+WJIn1erHZmMBNEVgvgEGUIG3mkPvW7w+ERyIgsDopUKWaYlRyhaiSwQICpyUyhociIjBbdlBn/9a3XVRWdY/tLMt/A/bC6LxFwU0EToXdbneZ4B1EXoGiuh+chrfHDHcBvhlJJZ5GUXSJW1qpVKvVy+Shnn48NN3hZg4z/+8cleH6zPRUoFGhaqQz2st90MIwLN03OLVyow79KlxuJtB/sInSFZoEtiWCPsGvZ2M+P9pI+KLnfWPSgzkm/B4kBwetYgZjE/9OqZ7KCXC5+5663QNf4jg5UxwSOA7BIfBERAmIDM62250GUFIpcJ+46CfFNQU97uqUtS4BTVwiskr2AveGFLeOLboqHo8ekMeqCAjonh8jcAF7nuQ1sOsrr5XJsiP1sgp2AXzwQjIS1Bv4hLsIisqlDdTmf9YiT/sWd90gpiv5mqbZO+z2JPcv3iY6agk5724AAAAASUVORK5CYII=";
        public static GUIContent ErrorIcon;

        private const string GroupIconData = "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";
        public static GUIContent GroupIcon;

        private const string ConnectorIconData = "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";
        public static GUIContent ConnectorIcon;

        private const string WireIconData = "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";
        public static GUIContent WireIcon;

        private const string DragIconData = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAB9UlEQVQ4EYWTvUodURDHz64LEY1RUgRCKkFTBmws8gYWYqMI8THyDHmCBCsLwcIqRRKSxioQ0igEorGQoJAiX4rF7r3n7u7Z3ZPf/3D3upFIBn57vmbmzMyZjawdGMn4+C0TRcY4V5kkScKePk3TsB+ZqqpGe93zeLR7NXnMdBsO4A2swc2iCIRu8r7Z8QhzX9f1V+hpzXhYFMUUGMF6RDAeOtiVMvIEcOZRLsU8/n7i9DfGd25ysKBbq6peB3N+fiEHc3KiyOq6OWVOJI3FwcxfEQwXL1EspSwjHLySAZKzd8Z4BItgWQ+wmWydhBukxMYXbaK0Bt4596gsy+CImxeBF3K30VVNTobpGV7BU2H/jc8Da61J0/QYpZz1bBzHKzzrrziOnoPJ86LX79uHPOs856tgDLeIFd2aZdk0g95cS2pSvec2j9ELMEJRwgVnH0Jv6DPkiINL7D7BR+ZPGb8zbkJr2I77nJ2BOs+1TBB6zaaqfa8sXSho11hPmGU9dWsf3qlr446CJZW72P8gx89kN0nOoY1DrlefpSQZm4BnYEJnyXOHGSIpVO3BIFd/tGFLZxl8mmZvgYJnoyK2xWzHKQxPaGq19SVRHoJTeshrCOlpjPQK/xL9gcgGT7nB/L4cMt9ib6+r/z8HBqNQB6Wi+XX5A29EX/Dt0W/eAAAAAElFTkSuQmCC";
        public static GUIContent DragIcon;

        private const string DuplicateIconData = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAABjklEQVQ4EXWSsUrDUBSGk/S2l+DUpZUiiloQXHwAt4Lo5NIHEBzcfQHfow6Ck5OLq0MHX0DcBAcpbbEdHOpQ0rSJ34m5lzSJB/6cc/7/5JyTe+NOJlNHbDb7SXz20W7vZVMbx3HsCMSUZYtBDeoWHBQl5x3uCixsg/y0+Xz+RMExeAHWZDDoQmyCM9vAVvwFW77vn7Jmk3Tquq6VV6uVxI1KxZtIUGgg38YLPtoIX9CVqpj3RiYQb6xBsKBJmE7VRsh54ZO1vFQ4x3+DN158wMsRF68FsmCyMoiDYHGzXC53CJtgHwxACyRXlvPCD4Uz4jQMwxow+TZizIHVoygyXOIlZ1BddMweYqiUarHep6zINgNcv1pVH/hn4bLmed5JEAR94cwZSGzvSuuaAzoU9jgT4aWheE0q6GmtO8B+woh1dmWlPFjXYe2jPG9yLw0UhWMmrJl8L7bBxFf84ZqYJuZHaTD8Gu6+pOhCOAYMSzTHTad0mXJHQVBSpMfjr0v4xxLNNpDft0xPOBr8q/0C9mP6ENvGxloAAAAASUVORK5CYII=";
        public static GUIContent DuplicateIcon;

        private const string DeleteIconData = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAACyklEQVQ4EW1Ty08TYRD/9kGytEJL2RZYqSUGLT5KebSxAUIC6s2YePERFC/+HWDkrP+AFwkJUQ+ePFkUYwUDFiGxSKF6aPqAlg20NAjJbnf9zaYcSJxkMr+Zbx7fNzMfF43O84wxYpMDQeoul5MpShtzOBxQGSuXyyyf32Z7eyVSRRMESb4mBRKgWF4URT0c7oPKhsAvwas1JjzY0xMgR518oVtxBMhIrAFK4A+9vcFYS4vniWEYEjFh2L7i7A354AIaxYBMbm7uswBQhUEcHR1OAyuJxMY45MzOToENDFxjqdRvqGwkGAx8qlar+fn5WAd0SiLyMBhNTU4WiYQ+wqjEYt+8hUJxBgyV8YuLS8xuPwPIecggCIISCvW9c7maGGKrvCw3U0P6bTbbcDK59cjtbs5CF8F1qGB0dJwDZJPB4NXXm5upsUTi102ns/EWbP0UyxWLu+Qw63bL9yF5PIn6UQes+XxeOpvs7Dz/NJPJTQI/83rPks0sFtVZyDGxsbGBDKHj4+MkARBHTTqpXAuegH2KDuFHYsPhaAgTwExJIGVNEvxf8NaW1UiGJ57y5w8OKgwcr6+XLqVSf+Agk8PJta3KtWB6FkunM0ySpK5KpfIdzMS1tZ9Mll3PPR75Ac5vgFvo2tlsbgKjnkomUyc9Mex2G+V4COYymewLVd23us2p6t5KuXzwvrv7SpQ8EHQP4i2SkMpTT9raWhkqtvv9F2YOD/9+8fsvrqjqEicKAs+jUnV5+cedkZGhNNZZ0TRtNxC4zHy+dsRzBp7H1teT45FIeFrX9Tx24zrisBO8wOEzUQpaaQ2NlDDvacz3LpU+OjrCZDiGBF2k45bReHz1NgrSKOoMwzSsBFB4JKFJaLSVoEFFaX2M32iNCr8xnsttv4J9oVQqoydWMPmZtHFEJrLRdxb390s6kizARnyKKBgkUmVI+k3mPwBeOqIDl2dDAAAAAElFTkSuQmCC";
        public static GUIContent DeleteIcon;

        private const string ConnectIconData = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAABr0lEQVQ4EX1SPUsDQRDd3TsjGCSF2CpY2NtZGgvtbEUbxSB+FFY24g+IdioipBI7m1hZiY2Vjb9AAhIsrJJokJO75G59b3MbL8nFgXfz+eZmd0e2Wi1BcRzHaH6iKBJKqa4fhhHyUkiphNZahGEIW5r8X1W3PNVYRvQReAYKPRWcgOBfLdrtdtdGrAho/PUL+MAEGvVl1hCG/E+DEsmQPUB4nkcs4kga5G02kCRT0u6g2fxeyuXGM0jfs4Z3EcurUtKFPTPsDuaRLGWzY3P481Oj8SkIx1G4XF6eHsVkHpB6hBPOTInH1/V64wIwL4DYGo8QBMEKMNDgjCTfDw6q1XdLOKrV6kW0o7/KPHAFmItOXuIpk5ACINDgGPqQhfTRYIN54JoxC9tggUk80y6XpEPQZhrGIZ1PFF1aotWSBMgLiqahJ+nEMZoTruvuQ8+CcAPNZeoR26CCaAbvOsXn5BpzVUHKAyPIPfSwkk487ibn9H0/z+XgeHDXoXnmW/rDgM36sbhjEzR4Q7FZWeiBM/c3kuAkB9qBs4UiH/Fz2OVkMs3ub2Bq+Je+xmlcE/sFDVEG0aNbL00AAAAASUVORK5CYII=";
        public static GUIContent ConnectIcon;

        private static GUIStyle _Header;
        public static GUIStyle Header
        {
            get
            {
                if (_Header == null)
                {
                    _Header = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true,
                        fontSize = 18,
                        fontStyle = FontStyle.Normal
                    };
                }

                return _Header;
            }
        }

        private static GUIStyle _Tab;
        public static GUIStyle Tab
        {
            get
            {
                if (_Tab == null)
                {
                    _Tab = new GUIStyle(EditorStyles.miniButtonMid)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        stretchWidth = true,
                        richText = true,
                        wordWrap = true,
                        fontSize = 12,
                        fixedHeight = 27.5f,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset()
                        {
                            left = 14,
                            right = 14,
                            top = 8,
                            bottom = 8
                        }
                    };
                }

                return _Tab;
            }
        }

        private static GUIStyle _Button;
        public static GUIStyle Button
        {
            get
            {
                if (_Button == null)
                {
                    _Button = new GUIStyle(UnityEngine.GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        stretchWidth = true,
                        richText = true,
                        wordWrap = true,
                        padding = new RectOffset()
                        {
                            left = 14,
                            right = 14,
                            top = 8,
                            bottom = 8
                        }
                    };
                }

                return _Button;
            }
        }


        private static GUIContent CreateIcon(string data)
        {
            byte[] bytes = System.Convert.FromBase64String(data);

            GUIContent c = new GUIContent();
            Texture2D icon = new Texture2D(iconWidth, iconHeight, TextureFormat.RGBA32, false, true);
            icon.LoadImage(bytes, true);
            c.image = icon;

            return c;
        }

        public static void DrawFooter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("- Staggart Creations -", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();
        }

        /*
        public static void DrawNetworkHeader(WireManager network)
        {
            if (!network) return;

            GUILayout.Space(5f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent(" Edit network", EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_editicon.sml" : "editicon.sml").image), EditorStyles.miniButton, GUILayout.Height(25f)))
                {
                    Selection.activeGameObject = network.gameObject;
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(5f);
        }
        */

        public static void DrawSceneEditorWiresButton()
        {
            Debug.Log("WireBuilderSceneGUI.Enabled = " + WireBuilderSceneGUI.Enabled);
            if (WireBuilderSceneGUI.Enabled) return;

            Rect pixelRect = EditorGUIUtility.PixelsToPoints(Camera.current.pixelRect);
            Rect windowRect = new Rect(pixelRect.width - 120 - 10, pixelRect.height - 30, 120, 60f);

            Handles.BeginGUI();
            GUILayout.BeginArea(windowRect);
            {
                if (GUILayout.Button(new GUIContent(" Edit Wires", WireBuilderGUI.GroupIcon.image), EditorStyles.toolbarButton))
                {
                    WireBuilderSceneGUI.Enable();
                }
            }
            GUILayout.EndArea();
            Handles.EndGUI();

        }

        public class ParameterGroup
        {
            static ParameterGroup()
            {
                Section = new GUIStyle(EditorStyles.textArea)
                {
                    margin = new RectOffset(0, 0, -10, 10),
                    padding = new RectOffset(10, 10, 10, 10),
                    clipping = TextClipping.Clip,
                };

                headerLabel = new GUIStyle(EditorStyles.miniLabel);
                headerBackgroundDark = new Color(0.1f, 0.1f, 0.1f, 0.2f);
                headerBackgroundLight = new Color(1f, 1f, 1f, 0.2f);
                paneOptionsIconDark = (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
                paneOptionsIconLight = (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");
                splitterDark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
                splitterLight = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            }

            public static readonly GUIStyle headerLabel;
            public static GUIStyle Section;
            static readonly Texture2D paneOptionsIconDark;
            static readonly Texture2D paneOptionsIconLight;
            public static Texture2D paneOptionsIcon { get { return EditorGUIUtility.isProSkin ? paneOptionsIconDark : paneOptionsIconLight; } }
            static readonly Color headerBackgroundDark;
            static readonly Color headerBackgroundLight;
            public static Color headerBackground { get { return EditorGUIUtility.isProSkin ? headerBackgroundDark : headerBackgroundLight; } }

            static readonly Color splitterDark;
            static readonly Color splitterLight;
            public static Color splitter { get { return EditorGUIUtility.isProSkin ? splitterDark : splitterLight; } }

            public static void DrawHeader(GUIContent content)
            {
                //DrawSplitter();
                Rect backgroundRect = GUILayoutUtility.GetRect(1f, 20f);

                if (content.image)
                {
                    content.text = " " + content.text;
                }

                Rect labelRect = backgroundRect;
                labelRect.y += 2f;
                labelRect.xMin += 5f;
                labelRect.xMax -= 20f;

                // Background rect should be full-width
                backgroundRect.xMin = 10f;
                //backgroundRect.width -= 10f;

                // Background
                EditorGUI.DrawRect(backgroundRect, headerBackground);

                // Title
                EditorGUI.LabelField(labelRect, content, EditorStyles.boldLabel);

                DrawSplitter();
            }

            public static void DrawSplitter()
            {
                var rect = GUILayoutUtility.GetRect(1f, 1f);

                // Splitter rect should be full-width
                rect.xMin = 10f;
                //rect.width -= 10f;

                if (Event.current.type != EventType.Repaint)
                    return;

                EditorGUI.DrawRect(rect, splitter);
            }


        }

        public class Scene
        {
            public static Color EditColor = new Color(0f, 0.5f, 1f, 1f);
            public static Color DeleteColor = new Color(1f, 0.25f, 0.25f, 1f);

            public static void SetColor(Color col)
            {
                Handles.color = new Color(col.r, col.g, col.b, Handles.color.a);
            }
            public static void SetOpacity(float a)
            {
                Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, a);
            }
            public const string mouseLabelSpace = "     ";
            public static float Sin()
            {
                return Mathf.Sin((float)EditorApplication.timeSinceStartup * 3.14159274f * 3f) * 0.5f + 0.5f;
            }

            public static void DrawMouseWorldLabel(Vector2 screenPos, GUIContent content)
            {
                DrawLabel(screenPos, content, false);
            }

            public static void DrawObjectLabel(Vector3 position, GUIContent content, bool error = false)
            {
                Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);

                DrawLabel(screenPos, content, error);
                
            }

            private static void DrawLabel(Vector2 screenPos, GUIContent content, bool error = false)
            {
                Handles.BeginGUI();


                Color baseColor = (error) ? Color.red : Color.white;

                Rect r = new Rect(screenPos.x, screenPos.y, Label.CalcSize(content).x, 30f);

                //Position right of position
                r.x += 25;
                r.y -= 20;

                if (content.image)
                {
                    GUI.color = EditorGUIUtility.isProSkin ? baseColor * 10f : Color.black;
                    GUI.Label(r, new GUIContent(content.image), Label);
                    r.width += content.image.width;
                    content.text = mouseLabelSpace + content.text;
                }

                GUI.color = EditorGUIUtility.isProSkin ? Color.black : Color.gray;
                if (EditorGUIUtility.isProSkin)
                {
                    GUI.Box(r, "", EditorStyles.helpBox);
                }
                else
                {
                    EditorGUI.DrawRect(r, ParameterGroup.headerBackground);
                }

                //Drop shadow
                r.x += 1;
                r.y += 1f;
                GUI.color = Color.black;
                GUI.Label(r, content.text, Label);

                if (EditorGUIUtility.isProSkin)
                {
                    GUI.color = baseColor * 1.33f;
                }
                else
                {
                    GUIStyle guiStyle = new GUIStyle();
                    guiStyle.normal.textColor = baseColor;
                    GUI.skin.customStyles[0] = guiStyle;
                }

                //Tex
                r.x -= 1;
                r.y -= 1f;
                GUI.Label(r, content.text, Label);
                Handles.EndGUI();
            }

            public static void DrawCircle(Vector3 position, float size, bool selected)
            {
                float circleSize = selected ? Mathf.Lerp(size * 1.1f, size, Sin()) : size;

                Handles.DrawSolidDisc(position, Camera.current.transform.forward, circleSize);
            }

            public static void DrawSquare(Vector3 position, float size, bool selected)
            {
                float circleSize = selected ? Mathf.Lerp(size * 1.1f, size, Sin()) : size;

                Handles.DotHandleCap(0, position, Quaternion.LookRotation(Camera.current.transform.forward, Vector3.up), circleSize, EventType.Repaint);
            }

            public static void VisualizeWirePoints(Wire wire)
            {
                if (SceneView.lastActiveSceneView == null) return;

                originalColor = Handles.color;
                Handles.color = Color.white;
                for (int i = 0; i < wire.points.Length; i++)
                {
                    Handles.DrawSolidDisc(wire.transform.position + wire.points[i], SceneView.lastActiveSceneView.camera.transform.forward, 0.05f);
                }
                Handles.color = originalColor;
            }

            public static void VisualizeWire(Wire wire)
            {
                if (wire.points == null) return;

                Vector3[] points = new Vector3[wire.points.Length];

                //Update positions of line renderer and such
                for (int i = 0; i < wire.points.Length; i++)
                {
                    //Transform position to world-space
                    points[i] = wire.gameObject.transform.TransformPoint(wire.points[i]);
                }

                Handles.color = EditColor * 1.5f;
                WireBuilderGUI.Scene.SetOpacity(0.5f);
                UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 3f, points);
            }

            public static void DrawDottedLine(Vector3 start, Vector3 end, float size = 1, int density = 2)
            {
                Camera sceneCam = SceneView.lastActiveSceneView.camera;

                if (sceneCam == null) return;

                float dist = Vector3.Distance(start, end);
                Vector3 dir = (end - start).normalized;

                int segmentLength = (int)dist;
                int dashes = segmentLength * density;

                Vector3[] points = new Vector3[dashes];

                for (int i = 0; i < points.Length; i++)
                {
                    //Sample point along wire length as 0-1 value
                    float s = (float)i / (float)(points.Length - 1);

                    points[i] = start + dir * segmentLength * s;

                    Handles.DrawSolidDisc(points[i], sceneCam.transform.forward, 0.1f * size);
                }
            }

            public static void DrawDashedLine(Vector3 start, Vector3 end, float screenSpaceSize)
            {
                m_DrawDashedLine(new Vector3[] { start, end }, screenSpaceSize);
            }

            public static void DrawDashedLine(Vector3[] lineSegments, float screenSpaceSize)
            {
                m_DrawDashedLine(lineSegments, screenSpaceSize);
            }

            private static void m_DrawDashedLine(Vector3[] lineSegments, float screenSpaceSize)
            {
                var dashSize = screenSpaceSize * UnityEditor.EditorGUIUtility.pixelsPerPoint;
                for (int i = 0; i < lineSegments.Length - 1; i += 2)
                {
                    var p1 = lineSegments[i + 0];
                    var p2 = lineSegments[i + 1];

                    if (p1 == Vector3.zero || p2 == Vector3.zero) continue;

                    UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, dashSize, new Vector3[] { p1, p2 });
                }
            }
            private static GUIStyle _Label;
            public static GUIStyle Label
            {
                get
                {
                    if (_Label == null)
                    {
                        _Label = new GUIStyle(EditorStyles.largeLabel)
                        {
                            alignment = TextAnchor.MiddleLeft,
                            fontSize = 12,
                            fontStyle = FontStyle.Bold,
                            padding = new RectOffset()
                            {
                                left = 10,
                                right = 0,
                                top = 0,
                                bottom = 0
                            }
                        };
                    }

                    return _Label;
                }
            }
        }
    }
}