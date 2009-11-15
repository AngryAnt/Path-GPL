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

namespace PathLibrary {

	[ Serializable() ]
	public class NavmeshAsset : NetworkAsset
	{
		private CellAsset[] cells;
		
		
		
		public NavmeshAsset( string name, CollectionAsset collection ) : base( name, collection )
		{
			cells = new CellAsset[ 0 ];
		}
		
		public NavmeshAsset( NavmeshAsset original ) : base( original.Name, original.Collection )
		{
			this.Position = original.Position;
			this.Size = original.Size;
			
			
			this.cells = new CellAsset[ original.Nodes.Length ];
			for( int i = 0; i < original.Nodes.Length; i++ )
			// Create new cell assets - based on the nodes of the original
			{
				this.cells[ i ] = new CellAsset( ( CellAsset )original.Nodes[ i ], this );
			}
			
			
			this.Tags = new int[ original.Tags.Length ];
			for( int i = 0; i < original.Tags.Length; i++ )
			// Copy the tag indexes
			{
				this.Tags[ i ] = original.Tags[ i ];
			}
			
			
			for( int cellIDX = 0; cellIDX < this.cells.Length; cellIDX++ )
			// Create connections going from OUR node at the same index as THEIR node to same - just with to
			{
				this.cells[ cellIDX ].Connections = new ConnectionAsset[ original.Nodes[ cellIDX ].Connections.Length ];
				for( int connectionIDX = 0; connectionIDX < this.cells[ cellIDX ].Connections.Length; connectionIDX++ )
				{
					this.cells[ cellIDX ].Connections[ connectionIDX ] = new ConnectionAsset(
						this.cells[ Array.IndexOf( original.Nodes, original.Nodes[ cellIDX ].Connections[ connectionIDX ].From ) ],
						this.cells[ Array.IndexOf( original.Nodes, original.Nodes[ cellIDX ].Connections[ connectionIDX ].To ) ],
						original.Nodes[ cellIDX ].Connections[ connectionIDX ].Width,
						this.Collection );
						
					this.cells[ cellIDX ].Connections[ connectionIDX ].Tags = new int[ original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags.Length ];
					for( int tagIDX = 0; tagIDX < original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags.Length; tagIDX++ )
					// Copy the tag indexes
					{
						this.cells[ cellIDX ].Connections[ connectionIDX ].Tags[ tagIDX ] = original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags[ tagIDX ];
					}
				}
			}
		}
		
		
		
		public override NetworkNodeAsset[] Nodes
		{
			get
			{
				return cells;
			}
		}
		
		
		
		public bool Add( CellAsset cell )
		{
			CellAsset[] newCells;
			newCells = new CellAsset[ cells.Length + 1 ];
			
			for( int i = 0; i < cells.Length; i++ )
			{
				if( cells[ i ].Name == cell.Name )
				{
					return false;
				}
				
				newCells[ i ] = cells[ i ];
			}
			
			newCells[ cells.Length ] = cell;
			cells = newCells;
			
			return true;
		}
		
		
		
		public bool Remove( CellAsset cell )
		{
			bool found;
			CellAsset[] newCells;
			
			found = false;
			newCells = new CellAsset[ cells.Length - 1 ];
			
			for( int i = 0; i < newCells.Length; i++ )
			{
				if( cells[ i ] == cell )
				{
					found = true;
				}

				newCells[ i ] = cells[ i + ( ( found ) ? 1 : 0 ) ];
			}
			
			found = ( cells[ newCells.Length ] == cell ) ? true : found;
			
			if( !found )
			{
				return false;
			}
			
			cells = newCells;

			return true;
		}

		
		
		public bool Generate( Mesh mesh )
		{
			Vector3[] points;
			Vector3 position, vector;
			TriangleAsset[] triangles;
			
			cells = new CellAsset[ mesh.triangles.Length / 3 ];
			
			for( int i = 0, cellID = 0; i < mesh.triangles.Length; i += 3, cellID++ )
			{
				triangles = new TriangleAsset[ 1 ];
				points = new Vector3[ 3 ];
				
				points[ 0 ] = mesh.vertices[ mesh.triangles[ i ] ];
				points[ 1 ] = mesh.vertices[ mesh.triangles[ i + 1 ] ];
				points[ 2 ] = mesh.vertices[ mesh.triangles[ i + 2 ] ];
				
				vector = points[ 0 ] - points[ 1 ];
				position = points[ 1 ] + vector.normalized * vector.magnitude / 2;

				vector = points[ 2 ] - position;
				position = position + vector.normalized * vector.magnitude / 2;
					
				triangles[ 0 ] = new TriangleAsset( points, null );
				
				cells[ cellID ] = new CellAsset( "Cell " + cellID, position, triangles, this, Collection );
			}
			
			return GenerateConnections();
		}
		
		
		
		public bool GenerateConnections()
		{
			bool success;
			
			success = true;
			foreach( CellAsset cell in cells )
			{
				success = ( GenerateConnections( cell ) ) ? success : false;
			}
			
			return success;
		}
		
		
		
		public bool GenerateConnections( CellAsset cell )
		{
			int matches;
			Vector3 matchOne, matchTwo;
			
			foreach( CellAsset other in cells )
			{
				if( other == cell )
				{
					continue;
				}
				
				matches = 0;
				foreach( TriangleAsset cellTriangle in cell.Triangles )
				{
					foreach( TriangleAsset otherTriangle in other.Triangles )
					{
						foreach( Vector3 cellPoint in cellTriangle.Points )
						{
							foreach( Vector3 otherPoint in otherTriangle.Points )
							{
								if( cellPoint == otherPoint )
								{
									matches++;
									if( matches == 0 )
									{
										matchOne = cellPoint;
									}
									else
									{
										matchTwo = cellPoint;
									}
								}
							}
						}
					}
				}
				
				if( matches == 2 )
				{
					cell.AddConnection( new ConnectionAsset( cell, other, Mathf.Abs( ( matchOne - matchTwo ).magnitude ), Collection ) );
					other.AddConnection( new ConnectionAsset( other, cell, Mathf.Abs( ( matchOne - matchTwo ).magnitude ), Collection ) );
				}
				else if( matches > 2 )
				{
					Debug.LogError( "More than two match points between two cells. What an odd mesh..." );
					return false;
				}
			}
			
			return true;
		}
		


		public void ReplaceConnections( ArrayList nodes, NetworkNodeAsset newNode )
		{
			NetworkNodeAsset[] theNodes;
			
			theNodes = new CellAsset[ nodes.Count ];
			
			for( int i = 0; i < nodes.Count; i++ )
			{
				theNodes[ i ] = ( NetworkNodeAsset )nodes[ i ];
			}
			
			ReplaceConnections( theNodes, newNode );
		}



		public void ReplaceConnections( NetworkNodeAsset[] nodes, NetworkNodeAsset newNode )
		// TODO: Move to parent
		{
			ConnectionAsset connection;			
					
			foreach( NetworkNodeAsset node in nodes )
			{
				foreach( ConnectionAsset existingConnection in node.Connections )
				{
					if( Array.IndexOf( nodes, existingConnection.To ) != -1 )
					{
						existingConnection.To = null;
						continue;
					}
					
					existingConnection.From = newNode;
					newNode.AddConnection( existingConnection );
				}
			}
			
			foreach( NetworkNodeAsset node in Nodes )
			{
				if( Array.IndexOf( nodes, node ) != -1 )
				{
					continue;
				}
				
				foreach( NetworkNodeAsset targetNode in nodes )
				{
					while( node.ConnectsTo( targetNode ) )
					{
						connection = node.GetConnection( targetNode );
						connection.To = newNode;
					}
				}
			}
		}

		
		
		public override void OnRenderGizmos( Transform transform, float standardMarkerSize )
		{
			base.OnRenderGizmos( transform, standardMarkerSize );
			
			foreach( CellAsset cell in Nodes )
			// Render all cells
			{
				cell.OnRenderGizmos( transform, standardMarkerSize, Position,
					( Editor.Instance != null && Editor.Instance.SelectedNetwork == this ),
					( Editor.Instance != null && Editor.Instance.SelectedNode.Contains( cell ) ),
					( Editor.Instance != null ) ? Editor.Instance.SelectedConnection : null
				);
			}
			
			if( Editor.Instance != null && Editor.Instance.SelectedNetwork == this && Editor.Instance.SelectedNode.Count != 0 )
			// Re-render selected cell
			{
				foreach( CellAsset cell in Editor.Instance.SelectedNode )
				{
					cell.OnRenderGizmos( transform, standardMarkerSize, Position, true, true, Editor.Instance.SelectedConnection );
				}
			}
		}
	}
}