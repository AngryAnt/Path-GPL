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


using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PathLibrary
{

	[ Serializable() ]
	public class GridNetworkAsset : NetworkAsset
	{
		GridNodeAsset[] nodes;
		
		
		public GridNetworkAsset( string name, CollectionAsset collection ) : base( name, collection )
		{
			nodes = new GridNodeAsset[ 0 ];
		}
		
	
		
		public GridNetworkAsset( GridNetworkAsset original ) : base( original.Name, original.Collection )
		{
			this.Position = original.Position;
			this.Size = original.Size;
			this.nodes = new GridNodeAsset[ original.Nodes.Length ];
			for( int i = 0; i < this.Nodes.Length; i++ )
			// Create new waypoint assets - based on the nodes of the original
			{
				this.Nodes[ i ] = new GridNodeAsset( ( GridNodeAsset )original.Nodes[ i ], this );
			}
			
			this.Tags = new int[ original.Tags.Length ];
			for( int i = 0; i < original.Tags.Length; i++ )
			// Copy the tag indexes
			{
				this.Tags[ i ] = original.Tags[ i ];
			}
			
			for( int cellIDX = 0; cellIDX < this.nodes.Length; cellIDX++ )
			// Create connections going from OUR node at the same index as THEIR node to same - just with to
			{
				this.nodes[ cellIDX ].Connections = new ConnectionAsset[ original.Nodes[ cellIDX ].Connections.Length ];
				for( int connectionIDX = 0; connectionIDX < this.nodes[ cellIDX ].Connections.Length; connectionIDX++ )
				{
					this.nodes[ cellIDX ].Connections[ connectionIDX ] = new ConnectionAsset(
						this.nodes[ Array.IndexOf( original.Nodes, original.Nodes[ cellIDX ].Connections[ connectionIDX ].From ) ],
						this.nodes[ Array.IndexOf( original.Nodes, original.Nodes[ cellIDX ].Connections[ connectionIDX ].To ) ],
						original.Nodes[ cellIDX ].Connections[ connectionIDX ].Width,
						this.Collection );
						
					this.nodes[ cellIDX ].Connections[ connectionIDX ].Tags = new int[ original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags.Length ];
					for( int tagIDX = 0; tagIDX < original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags.Length; tagIDX++ )
					// Copy the tag indexes
					{
						this.nodes[ cellIDX ].Connections[ connectionIDX ].Tags[ tagIDX ] = original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags[ tagIDX ];
					}
				}
			}
		}
		
		
		
		public override NetworkNodeAsset[] Nodes
		{
			get
			{
				return nodes;
			}
		}
		
		
		
		public GridNodeAsset Get( string name )
		{
			foreach( GridNodeAsset asset in nodes )
			{
				if( asset.Name == name )
				{
					return asset;
				}
			}
			
			return null;
		}
		
		
		
		public GridNodeAsset Add( GridNodeAsset node )
		{
			GridNodeAsset[] newNodes;
			newNodes = new GridNodeAsset[ nodes.Length + 1 ];
			
			for( int i = 0; i < nodes.Length; i++ )
			{
				if( nodes[ i ].Name == node.Name )
				{
					return null;
				}
				
				newNodes[ i ] = nodes[ i ];
			}
			
			newNodes[ nodes.Length ] = node;
			nodes = newNodes;
			
			return node;
		}
		
		
		
		public bool Remove( GridNodeAsset node )
		{
			bool found;
			GridNodeAsset[] newNodes;
			
			found = false;
			newNodes = new GridNodeAsset[ nodes.Length - 1 ];
			
			for( int i = 0; i < newNodes.Length; i++ )
			{
				if( nodes[ i ] == node )
				{
					found = true;
				}

				newNodes[ i ] = nodes[ i + ( ( found ) ? 1 : 0 ) ];
			}
			
			found = ( nodes[ newNodes.Length ] == node ) ? true : found;
			
			if( !found )
			{
				return false;
			}
			
			nodes = newNodes;

			return true;
		}
		
		
		
		public void Disconnect()
		{
			foreach( GridNodeAsset node in nodes )
			{
				node.Disconnect();
			}
		}
		
		
		
		public override void OnRenderGizmos( Transform transform, float standardMarkerSize )
		{
			foreach( GridNodeAsset node in Nodes )
			// Render all nodes and connections
			{
				if( node.Target == null )
				{
					continue;
				}
				
				node.OnRenderGizmos( transform, standardMarkerSize, node.Target.Network.Position,
					( Editor.Instance != null && Editor.Instance.SelectedNetwork == this ),
					( Editor.Instance != null && Editor.Instance.SelectedNode.Contains( node ) ),
					( Editor.Instance != null ) ? Editor.Instance.SelectedConnection : null
				);
			}
			
			if( Editor.Instance != null && Editor.Instance.SelectedNetwork == this && Editor.Instance.SelectedNode.Count == 1 && Editor.Instance.SelectedConnection != null )
			// Re-render selected node
			{
				( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).OnRenderGizmos( transform, standardMarkerSize,
					( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target.Network.Position,
					( Editor.Instance.SelectedNetwork == this ), true, Editor.Instance.SelectedConnection
				);
			}
			
			if( Editor.Instance != null && Editor.Instance.SelectedNetwork == this && Editor.Instance.SelectedNode.Count == 1 && Editor.Instance.SelectedConnection != null )
			// Re-render selected connection
			{
				( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).OnRenderGizmos( transform, standardMarkerSize,
					( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target.Network.Position,
					true,
					true,
					Editor.Instance.SelectedConnection,
					true
				);
			}
		}
	}

}