/*
    Path - pathfinding system for the Unity engine

    Copyright (C) 2008 Emil E. Johansen

    This file is part of Path.

    Path is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 2 of the License, or
    (at your option) any later version.

    Path is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Path.  If not, see <http://www.gnu.org/licenses/>.

    For alternative license options, contact the copyright holder.

    Emil E. Johansen emil@eej.dk
*/


using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Collections;

namespace PathLibrary
{
	public class Resources
	{
		private static GUIStyle list, selectedList, unfocusedSelectedList, listItem, selectedListItem, unfocusedSelectedListItem, textLine, title, toolbarPullDownUp, wrappedLabel, wrappedMiniLabel;
		private static Texture2D logo, logoShadow, pathLogo, pathLogoWhite, scale, network, waypoint, connection, collapsed, expanded, titleBackground, help, tag;
		
		
		
		public static Stream GetResourceStream( string resourceName, Assembly assembly )
		{
			if( assembly == null )
			{
				assembly = Assembly.GetExecutingAssembly();
			}
			
			return assembly.GetManifestResourceStream( resourceName );
		}
		
		
		
		public static Stream GetResourceStream( string resourceName )
		{
			return GetResourceStream( resourceName, null );
		}
		
		
		
		public static byte[] GetByteResource( string resourceName, Assembly assembly )
		{
			Stream byteStream;
			byte[] buffer;
			
			byteStream = GetResourceStream( resourceName, assembly );
			buffer = new byte[ byteStream.Length ];
			byteStream.Read( buffer, 0, ( int )byteStream.Length );
			byteStream.Close();
			
			return buffer;
		}
		
		
		
		public static byte[] GetByteResource( string resourceName )
		{
			return GetByteResource( resourceName, null );
		}
		
		
		
		public static Texture2D GetTextureResource( string resourceName, Assembly assembly )
		{
			Texture2D texture;
		
			texture = new Texture2D( 4, 4 );
			texture.LoadImage( GetByteResource( resourceName, assembly ) );
			
			return texture;
		}
		
		
		
		public static Texture2D GetTextureResource( string resourceName )
		{
			return GetTextureResource( resourceName, null );
		}
		
		
		
		public static string Version
		{
			get
			{
				#if DEBUG
					return "Path 1.0.1 DEBUG";
				#else
					return "Path 1.0.1";
				#endif
			}
		}
		
		
		
		public static string License
		{
			get
			{
				#if DEBUG
					return "Path.dll - distribution of the 'Path library' pathfinding system\nversion 1.0.1 DEBUG, March, 2009\n\nCopyright (C) AngryAnt, Emil Johansen\n\nThis software is provided 'as-is', without any express or implied\nwarranty.  In no event will the author be held liable for any damages\narising from the use of this software.\n\nPermission is granted to anyone to use this software for any purpose,\nincluding commercial applications, and to alter it and redistribute it\nfreely, subject to the following restrictions:\n\n1. The origin of this software must not be misrepresented; you must not\n   claim that you wrote the original software. If you use this software\n   in a product, an acknowledgment is required as stated in point 2 of\n   this notice.\n2. If you use this software in a product, you are required to acknowledge\n   its original creator by displaying at least one unaltered version of the\n   provided software logo images in your product splash screen for no less\n   than four full seconds. If your product has an \"About\" or credits view,\n   this same logo image must also be displayed there. This requirement may\n   be waived for an additional fee. Contact the original software author\n   for details.\n3. The original software author may, in any customer reference list or in\n   any press release, use the name of products, individuals and companies\n   using this software.\n4. This Agreement will be governed by the laws of the State of Denmark as\n   they are applied to agreements between Denmark residents entered into\n   and to be performed entirely within Denmark. The United Nations\n   Convention on Contracts for the International Sale of Goods is\n   specifically disclaimed.\n5. Whether you are licensing the Software as an individual or on behalf of\n   an entity, you may not: (a) reverse engineer, decompile, or disassemble\n   the Software or attempt to discover the source code; (b) modify the\n   Software runtime in whole or in part without the express written consent\n   of the original software creator; (c) remove any proprietary notices or\n   labels on the Software; (d) resell, lease, rent, transfer, sublicense,\n   or otherwise transfer rights to the Software.\n6. You agree that this is the entire agreement between you and the original\n   software creator, which supersedes any prior agreement, whether written\n   or oral, and all other communications between the original software\n   creator and you relating to the subject matter of this Agreement.\n7. This notice and the provided logo image files may not be removed or\n   altered from any source distribution.\n\nAngryAnt, Emil Johansen emil@eej.dk";
				#else
					return "Path.dll - distribution of the 'Path library' pathfinding system\nversion 1.0.1, March, 2009\n\nCopyright (C) AngryAnt, Emil Johansen\n\nThis software is provided 'as-is', without any express or implied\nwarranty.  In no event will the author be held liable for any damages\narising from the use of this software.\n\nPermission is granted to anyone to use this software for any purpose,\nincluding commercial applications, and to alter it and redistribute it\nfreely, subject to the following restrictions:\n\n1. The origin of this software must not be misrepresented; you must not\n   claim that you wrote the original software. If you use this software\n   in a product, an acknowledgment is required as stated in point 2 of\n   this notice.\n2. If you use this software in a product, you are required to acknowledge\n   its original creator by displaying at least one unaltered version of the\n   provided software logo images in your product splash screen for no less\n   than four full seconds. If your product has an \"About\" or credits view,\n   this same logo image must also be displayed there. This requirement may\n   be waived for an additional fee. Contact the original software author\n   for details.\n3. The original software author may, in any customer reference list or in\n   any press release, use the name of products, individuals and companies\n   using this software.\n4. This Agreement will be governed by the laws of the State of Denmark as\n   they are applied to agreements between Denmark residents entered into\n   and to be performed entirely within Denmark. The United Nations\n   Convention on Contracts for the International Sale of Goods is\n   specifically disclaimed.\n5. Whether you are licensing the Software as an individual or on behalf of\n   an entity, you may not: (a) reverse engineer, decompile, or disassemble\n   the Software or attempt to discover the source code; (b) modify the\n   Software runtime in whole or in part without the express written consent\n   of the original software creator; (c) remove any proprietary notices or\n   labels on the Software; (d) resell, lease, rent, transfer, sublicense,\n   or otherwise transfer rights to the Software.\n6. You agree that this is the entire agreement between you and the original\n   software creator, which supersedes any prior agreement, whether written\n   or oral, and all other communications between the original software\n   creator and you relating to the subject matter of this Agreement.\n7. This notice and the provided logo image files may not be removed or\n   altered from any source distribution.\n\nAngryAnt, Emil Johansen emil@eej.dk";
				#endif
			}
		}
		
		
		
		public static string Thanks
		{
			get
			{
				return "Ann-Louise Salomonsen, family and friends, unity technologies, Charles Hinshaw, Ricardo J. Mendez, the unity community and the creatures of the #unity3d freenode irc channel";
			}
		}
		
		
		
		public static string Copyright
		{
			get
			{
				return "Copyright (C) Emil Johansen - AngryAnt 2009";
			}
		}
		
		
		
		public static Texture2D Logo
		{
			get
			{
				if( logo == null )
				{
					logo = GetTextureResource( "Logo.png" );
				}

				return logo;
			}
		}
		
		
		
		public static Texture2D LogoShadow
		{
			get
			{
				if( logoShadow == null )
				{
					logoShadow = GetTextureResource( "LogoShadow.png" );
				}

				return logoShadow;
			}
		}
		
		
		
		public static Texture2D PathLogo
		{
			get
			{
				if( pathLogo == null )
				{
					pathLogo = GetTextureResource( "Path.png" );
				}

				return pathLogo;
			}
		}
		
		
		
		public static Texture2D PathLogoWhite
		{
			get
			{
				if( pathLogoWhite == null )
				{
					pathLogoWhite = GetTextureResource( "PathWhite.png" );
				}

				return pathLogoWhite;
			}
		}
		
		
		
		public static Texture2D Scale
		{
			get
			{
				if( scale == null )
				{
					scale = GetTextureResource( "Scale.png" );
				}

				return scale;
			}
		}
		
		
		
		public static Texture2D Network
		{
			get
			{
				if( network == null )
				{
					network = GetTextureResource( "Network.png" );
				}

				return network;
			}
		}
		
		
		
		public static Texture2D Waypoint
		{
			get
			{
				if( waypoint == null )
				{
					waypoint = GetTextureResource( "Waypoint.png" );
				}

				return waypoint;
			}
		}
		
		
		
		public static Texture2D Connection
		{
			get
			{
				if( connection == null )
				{
					connection = GetTextureResource( "Connection.png" );
				}

				return connection;
			}
		}
		
		
		
		public static Texture2D Collapsed
		{
			get
			{
				if( collapsed == null )
				{
					collapsed = GetTextureResource( "Collapsed.png" );
				}

				return collapsed;
			}
		}
		
		
		
		public static Texture2D Expanded
		{
			get
			{
				if( expanded == null )
				{
					expanded = GetTextureResource( "Expanded.png" );
				}

				return expanded;
			}
		}
		
		
		
		public static Texture2D TitleBackground
		{
			get
			{
				if( titleBackground == null )
				{
					titleBackground = GetTextureResource( "TitleBackground.png" );
				}

				return titleBackground;
			}
		}
		
		
		
		public static Texture2D Help
		{
			get
			{
				if( help == null )
				{
					help = GetTextureResource( "_Help.png" );
				}

				return help;
			}
		}
        
		
		
		public static Texture2D Tag
		{
			get
			{
				if( tag == null )
				{
					tag = GetTextureResource( "Tag.png" );
				}

				return tag;
			}
		}
		
		
		
		public static GUIStyle ListStyle
		{
            get
            {
	            if( list == null )
	            {
	                list = new GUIStyle();
	            }
                return list;
            }
		}
	
	
	
        public static GUIStyle SelectedListStyle
        {
            get
            {
	            if( selectedList == null )
	            {
	                selectedList = new GUIStyle();
	                selectedList.normal.background = GetTextureResource( "SelectedListItem.png" );
	            }
                return selectedList;	
            }
        }



        public static GUIStyle UnfocusedSelectedListStyle
        {
            get
            {
	            if( unfocusedSelectedList == null )
	            {
	                unfocusedSelectedList = new GUIStyle();
	                unfocusedSelectedList.normal.background = GetTextureResource( "UnfocusedSelectedListItem.png" );
					unfocusedSelectedList.normal.textColor = Color.white;
	            }
                return unfocusedSelectedList;	
            }
        }



		public static GUIStyle ListItemStyle
		{
			get
			{
				if( listItem == null )
				{
					listItem = new GUIStyle();
					listItem.padding.left = 5;
					listItem.padding.right = 5;
					listItem.padding.top = 5;
					listItem.padding.bottom = 5;
				}
				return listItem;
			}
		}
		
		
		
		public static GUIStyle SelectedListItemStyle
		{
			get
			{
				if( selectedListItem == null )
				{
					selectedListItem = new GUIStyle();
	                selectedListItem.normal.background = GetTextureResource( "SelectedListItem.png" );
					selectedListItem.padding.left = 5;
					selectedListItem.padding.right = 5;
					selectedListItem.padding.top = 5;
					selectedListItem.padding.bottom = 5;
				}
				return selectedListItem;
			}
		}
		
		
		
		public static GUIStyle UnfocusedSelectedListItemStyle
		{
			get
			{
				if( unfocusedSelectedListItem == null )
				{
					unfocusedSelectedListItem = new GUIStyle();
	                unfocusedSelectedListItem.normal.background = GetTextureResource( "UnfocusedSelectedListItem.png" );
	                unfocusedSelectedListItem.normal.textColor = Color.white;
					unfocusedSelectedListItem.padding.left = 5;
					unfocusedSelectedListItem.padding.right = 5;
					unfocusedSelectedListItem.padding.top = 5;
					unfocusedSelectedListItem.padding.bottom = 5;
				}
				return unfocusedSelectedListItem;
			}
		}



        public static GUIStyle TextLineStyle
        {
            get
            {
	            if( textLine == null )
	            {
	                textLine = new GUIStyle();
	                textLine.normal.background = GetTextureResource( "TextLine.png" );
	                textLine.normal.textColor = Color.white;
	            }
                return textLine;	
            }
        }



		public static GUIStyle TitleStyle
		{
			get
			{
				if( title == null )
				{
					title = new GUIStyle( "Box" );
	                //title.normal.background = TitleBackground;
					title.padding.left = title.padding.right = title.padding.top = title.padding.bottom = 5;
					title.margin.left = title.margin.right = title.margin.top = title.margin.bottom = 0;
				}
				return title;
			}
		}



		public static GUIStyle ToolbarPullDownUpStyle
		{
			get
			{
				if( toolbarPullDownUp == null )
				{
					toolbarPullDownUp = new GUIStyle( GUI.skin.GetStyle( "ToolbarPullDown" ) );
					toolbarPullDownUp.name = "ToolbarPullDownUp";
					toolbarPullDownUp.hover = toolbarPullDownUp.normal;
					toolbarPullDownUp.active = toolbarPullDownUp.normal;
					toolbarPullDownUp.onNormal = toolbarPullDownUp.normal;
					toolbarPullDownUp.onHover = toolbarPullDownUp.normal;
					toolbarPullDownUp.onActive = toolbarPullDownUp.normal;
					toolbarPullDownUp.focused = toolbarPullDownUp.normal;
					toolbarPullDownUp.onFocused = toolbarPullDownUp.normal;
				}
				return toolbarPullDownUp;
			}
		}
		
		
		
		public static GUIStyle WrappedLabelStyle
		{
			get
			{
				if( wrappedLabel == null )
				{
					wrappedLabel = new GUIStyle( GUI.skin.GetStyle( "Label" ) );
					wrappedLabel.wordWrap = true;
				}
				return wrappedLabel;
			}
		}
		
		
		
		public static GUIStyle WrappedMiniLabelStyle
		{
			get
			{
				if( wrappedMiniLabel == null )
				{
					wrappedMiniLabel = new GUIStyle( GUI.skin.GetStyle( "MiniLabel" ) );
					wrappedMiniLabel.wordWrap = true;
				}
				return wrappedMiniLabel;
			}
		}
		
		
		
		public static float FloatField( float value )
		{
			string text;
			
			text = "" + value;
			text = GUILayout.TextField( text );
			
			if( text != "" )
			{
				return System.Single.Parse( text );
			}
			
			return 0.0f;
		}
		
		
		
		public static Vector3 Vector3Field( Vector3 vector )
		{
			GUILayout.BeginHorizontal();
				vector = new Vector3( FloatField( vector.x ), FloatField( vector.y ), FloatField( vector.z ) );
			GUILayout.EndHorizontal();
			
			return vector;
		}



		public static int DefaultListItemHeight
		{
			get
			{
				return 14;
			}
		}



		public static float DefaultLeftColumnWidth
		{
			get
			{
				return 100.0f;
			}
		}


		public static bool MultiSelectKey
		{
			get
			{
				switch( Application.platform )
				{
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
					case RuntimePlatform.OSXWebPlayer:
					case RuntimePlatform.OSXDashboardPlayer:
						return Event.current.command;
					default:
						return Event.current.control;
				}
			}
		}
		
		
		
		public delegate bool OnListItemGUI( object item, bool selected, ICollection list );
		
		
		
 		private static bool OnDefaultListItemGUI( object item, bool selected, ICollection list )
		{
			if( GUILayout.Button( item.ToString(), selected ? SelectedListStyle : ListStyle ) )
			{					
				selected = !selected;
			}
			
			return selected;
		}



		/*public static ICollection SelectList( ICollection list, ICollection selected, int maxSelection, OnListItemGUI itemHandler )
		{
			ArrayList newSelection;
			
			newSelection = new ArrayList( selected );
			
			foreach( object item in list )
			{
				if( itemHandler( item, newSelection.Contains( item ), list ) )
				// If the item is now/still selected
				{
					if( !newSelection.Contains( item ) )
					{
						if( MultiSelectKey )
						{
							if( newSelection.Count < maxSelection )
							{
								newSelection.Add( item );
							}
						}
						else
						{
							newSelection.Clear();
							newSelection.Add( item );
						}
					}
				}
				else if( newSelection.Contains( item ) )
				// If the item has just been deselected
				{
					if( MultiSelectKey )
					{
						newSelection.Remove( item );
					}
					else
					{
						newSelection.Clear();
					}
				}
			}
		
			return newSelection;
		}*/
		
		
		
		public static ICollection SelectList( ICollection list, ICollection selected, int maxSelection, OnListItemGUI itemHandler )
		{
			object[] oldSelection;
			ArrayList newSelection = new ArrayList( maxSelection );

			oldSelection = new object[ selected.Count ];
			selected.CopyTo( oldSelection, 0 );

			foreach( object item in list )
			{
				if( itemHandler( item, System.Array.IndexOf( oldSelection, item ) > -1, list ) )
				// If the item is now/still selected
				{
					if( MultiSelectKey )
					{
						if( newSelection.Count < maxSelection )
						{
							newSelection.Add( item );
						}
					}
					else
					{
						//newSelection.Clear(); //not arsure if needed
						newSelection.Add( item );
					}
				}
			}

			return newSelection;
		}



		public static object SelectList( ICollection list, object selected, OnListItemGUI itemHandler )
		{
			ArrayList selectionList;
			
			selectionList = new ArrayList();
			
			if( selected != null )
			{
				selectionList.Add( selected );
			}
			
			selectionList = new ArrayList( SelectList( list, selectionList, 1, itemHandler ) );
		
			return ( selectionList.Count > 0 ) ? selectionList[ 0 ] : null;
		}

		
		
		public static ICollection SelectList( ICollection list, ICollection selected, int maxSelection )
		{
			return SelectList( list, selected, maxSelection, OnDefaultListItemGUI );
		}
		
		
		
		public static object SelectList( ICollection list, object selected )
		{
			return SelectList( list, selected, OnDefaultListItemGUI );
		}



		public static object PulldownPopup( string label, ArrayList list, string emptyString )
		{
			string[] popupList;
			int popupSelection;
			
			popupSelection = 0;
			
			if( list.Count == 0 )
			{
				popupList = new string[ 2 ];
				popupList[ 0 ] = label;
				popupList[ 1 ] = emptyString;
				
				EditorGUILayout.Popup( 0, popupList/*, Resources.ToolbarPullDownUpStyle*/ );
			}
			else
			{
				popupList = new string[ list.Count + 1 ];
				popupList[ 0 ] = label;
				
				for( int i = 0; i < list.Count; i++ )
				{
					if( list[ i ] == null )
					{
						continue;
					}
					
					popupList[ i + 1 ] = list[ i ].ToString();
				}
				
				popupSelection = EditorGUILayout.Popup( popupSelection, popupList/*, Resources.ToolbarPullDownUpStyle*/ );
				
				if( popupSelection != 0 )
				{
					return list[ popupSelection - 1 ];
				}
			}
			
			return null;
		}
		
		
		
		public static ArrayList LayerNames
		{
			get
			{
				ArrayList layers;
				string layer;
				
				layers = new ArrayList();
				for( int i = 0; i <= 31; i++ )
				{
					layer = LayerMask.LayerToName( i );
					if( layer != "" )
					{
						layers.Add( layer );
					}
				}
				
				return layers;
			}
		}


		
		public static LayerMask LayerMaskField( LayerMask value )
		{
			ArrayList layers;
			string[] popupList;
			int index;
			
			layers = LayerNames;
			layers.Insert( 0, "All" );
			
			popupList = new string[ layers.Count ];
			
			for( int i = 0; i < layers.Count; i++ )
			{
				popupList[ i ] = layers[ i ].ToString();
			}
			
			index = ( value == -1 ) ? 0 : System.Array.IndexOf( popupList, LayerMask.LayerToName( value ) );
			index = EditorGUILayout.Popup( index, popupList );
			value = ( index > 0 ) ? LayerMask.NameToLayer( popupList[ index ] ) : -1;
			
			return value;
		}
		


		public static Vector3 About( Vector3 scroll )
		{
			GUILayout.BeginHorizontal();
				GUILayout.Label( Resources.LogoShadow );
				GUILayout.BeginVertical();
					GUILayout.BeginVertical( GUILayout.Height( 70.0f ) );
						GUILayout.FlexibleSpace();
						GUILayout.Label( Resources.PathLogo );
						GUILayout.Label( Resources.Version );
					GUILayout.EndVertical();
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Space( 20.0f );
			//GUILayout.Label( "", TextLineStyle, GUILayout.Height( 1.0f ) );
			
			GUILayout.BeginHorizontal();
				GUILayout.Space( 5.0f );
				GUILayout.BeginVertical();
					scroll = GUILayout.BeginScrollView( scroll, GUI.skin.GetStyle( "Box" ) );
						GUILayout.Label( Resources.License, Resources.WrappedMiniLabelStyle );
					GUILayout.EndScrollView();
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			//GUILayout.Label( "", TextLineStyle, GUILayout.Height( 1.0f ) );
			GUILayout.FlexibleSpace();
			GUILayout.Space( 20.0f );
					
			GUILayout.BeginHorizontal();
				GUILayout.Space( 5.0f );
				GUILayout.BeginVertical();
					GUILayout.Label( "Thanks to " + Resources.Thanks + ".", Resources.WrappedLabelStyle );
					GUILayout.Space( 10.0f );
					GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						GUILayout.Label( Resources.Copyright );
					GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUILayout.Space( 5.0f );
			GUILayout.EndHorizontal();
			
			return scroll;
		}



        public static void Documentation( string topic )
        {
			System.Diagnostics.Process browserProcess;
			
			try
			{
				browserProcess = new System.Diagnostics.Process();
				browserProcess.EnableRaisingEvents = false;
				
				switch( Application.platform )
				{
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
					case RuntimePlatform.OSXWebPlayer:
					case RuntimePlatform.OSXDashboardPlayer:
						browserProcess.StartInfo.FileName = ( ( topic == "" ) ? "open http://angryant.com/pathdoc.html" : ( "open http://angryant.com/pathdoc.html#" + topic ) );
					break;
					default:
						browserProcess.StartInfo.FileName = ( ( topic == "" ) ? "start http://angryant.com/pathdoc.html" : ( "start http://angryant.com/pathdoc.html#" + topic ) );
					break;
				}
				
				browserProcess.Start();
			}
			catch( System.Exception e )
			{
                Debug.LogError( "Unable to launch help documentation: " + e );
			}
        }



        public static void Documentation()
		{
			Documentation( "" );
		}



		public static int[] SharedTags( ICollection assets )
		{
			int[] result;
			
			ArrayList tags;
			bool shared;
			
			tags = new ArrayList();
			foreach( TaggedAsset asset in assets )
			{
				foreach( int tag in asset.Tags )
				{
					if( tags.Contains( tag ) )
					{
						continue;
					}
					
					shared = true;
					
					foreach( TaggedAsset other in assets )
					{
						if( !other.HasTag( tag ) )
						{
							shared = false;
							break;
						}
					}
					
					if( shared )
					{
						tags.Add( tag );
					}
				}
			}
			
			result = new int[ tags.Count ];
			System.Array.Copy( tags.ToArray(), result, tags.Count );
			
			return result;
		}
	}
}