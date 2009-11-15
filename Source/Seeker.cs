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
using System.Collections;

namespace PathLibrary
{
	public interface ISearchMonitor
	{
		void OnSearchCompleted( Seeker seeker );
		void OnSearchFailed( Seeker seeker );
		void OnSeekerInvalidated( Seeker seeker );
	}
	
	public class Seeker
	{
		private Vector3 from, to;
		private NetworkNodeAsset start, end;
		private ArrayList monitors, solution;
		private float maxFrameTime, radius;
		private object data;
		private string[] requiredTags, excludedTags;
		private bool validateNetworks, seeking;
		private float cacheLifespan;
		
		
		
		public Seeker( Vector3 from, Vector3 to, float maxFrameTime, float radius, object data )
		{
			this.from = from;
			this.to = to;
			this.maxFrameTime = maxFrameTime;
			this.radius = radius;
			this.data = data;
			requiredTags = new string[ 0 ];
			excludedTags = new string[ 0 ];
			validateNetworks = false;
			
			start = Control.Instance.NearestNode( from, this );
			end = Control.Instance.NearestNode( to, this );
			monitors = new ArrayList();
			solution = new ArrayList();
			seeking = false;
			
			cacheLifespan = Control.NoCache;
			
			Control.Instance.RegisterSeeker( this );
		}
		
		
		
		public Seeker( Vector3 from, Vector3 to, float maxFrameTime, float radius, string[] requiredTags, string[] excludedTags, bool validateNetworks, object data ) : this( from, to, maxFrameTime, radius, data )
		{
			this.requiredTags = requiredTags;
			this.excludedTags = excludedTags;
			this.validateNetworks = validateNetworks;
		}
		
		
		
		public NetworkNodeAsset Start
		{
			get
			{
				return start;
			}
		}
		
		
		
		public NetworkNodeAsset End
		{
			get
			{
				return end;
			}
		}
		
		
		
		public Vector3 From
		{
			get
			{
				return from;
			}
		}
		
		
		
		public Vector3 To
		{
			get
			{
				return to;
			}
		}
		
		
		
		public object Data
		{
			get
			{
				return data;
			}
			set
			{
				data = value;
			}
		}
		
		
		
		public float CacheLifespan
		{
			get
			{
				return cacheLifespan;
			}
			set
			{
				cacheLifespan = value;
			}
		}
		
		
		
		public bool Seeking
		{
			get
			{
				return seeking;
			}
		}
		
		
		
		public bool ValidateNetworks
		{
			get
			{
				return validateNetworks;
			}
			set
			{
				validateNetworks = value;
			}
		}
		
		
		
		public ArrayList Solution
		{
			get
			{
				return solution;
			}
		}
		
		
		
		public bool DoesUse( NetworkAsset network )
		{
			if( end.Network == network )
			{
				return true;
			}
			
			foreach( ConnectionAsset connection in Solution )
			{
				if( connection.From.Network == network )
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		
		public bool DoesUse( NetworkNodeAsset node )
		{
			if( end == node )
			{
				return true;
			}
			
			foreach( ConnectionAsset connection in Solution )
			{
				if( connection.From == node )
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		
		public bool DoesUse( ConnectionAsset connection )
		{
			foreach( ConnectionAsset connectionAsset in Solution )
			{
				if( connectionAsset == connection )
				{
					return true;
				}
			}
			
			return false;
		}

		
		
		public void AddMonitor( ISearchMonitor monitor )
		{
			if( !monitors.Contains( monitor ) )
			{
				monitors.Add( monitor );
			}
		}
		
		
		
		public void RemoveMonitor( ISearchMonitor monitor )
		{
			monitors.Remove( monitor );
		}		
		
		
		
		private float GScore( ConnectionAsset connection )
		{
			return connection.Cost;
		}
		
		
		
		private float HScore( ConnectionAsset connection )
		{
			return Vector3.Distance( connection.To.Position, End.Position );
		}
		
		
		
		private class PathData : System.IComparable
		{
			private float gScore, hScore;
			private ArrayList path;
			
			public PathData( ConnectionAsset connection, float gScore, float hScore )
			{
				this.path = new ArrayList();
				this.path.Add( connection );
				this.gScore = gScore;
				this.hScore = hScore;
			}
			
			public PathData( PathData pathData, ConnectionAsset connection, float gScore, float hScore )
			{
				this.path = new ArrayList( pathData.Path );
				this.path.Add( connection );
				this.gScore = pathData.GScore + gScore;
				this.hScore = pathData.HScore + hScore;
			}
			
			public float GScore
			{
				get
				{
					return gScore;
				}
			}
			
			public float HScore
			{
				get
				{
					return hScore;
				}
			}
			
			public float FScore
			{
				get
				{
					return GScore + HScore;
				}
			}
			
			public ArrayList Path
			{
				get
				{
					return path;
				}
			}
			
			public ConnectionAsset Connection
			{
				get
				{
					return ( ConnectionAsset )path[ path.Count - 1 ];
				}
			}
			
			public NetworkNodeAsset Destination
			{
				get
				{
					return Connection.To;
				}
			}
			
			public ICollection Connections
			{
				get
				{
					ArrayList allConnections, properConnections, gridNodes;
					
					allConnections = new ArrayList( Destination.Connections );
					gridNodes = Control.Instance.GetGridNodes( Destination );
						// Get all grid nodes which target our current destination
					
					if( Destination is GridNodeAsset )
					// If we're in a grid network, also consider the connections of the target
					{
						allConnections.AddRange( ( ( GridNodeAsset )Destination).Target.Connections );
					}
					
					foreach( GridNodeAsset gridNode in gridNodes )
					// Add grid network connections for all grid nodes targeting our current destination
					{
						allConnections.AddRange( gridNode.Connections );
					}
					
					if( path.Count < 2 )
					// If we've only got one node, we needn't do backtracking tests
					{
						return allConnections;
					}
					
					properConnections = new ArrayList();
					foreach( ConnectionAsset connection in allConnections )
					// Test for backtracking (connection destination equalling connection origin of last connection)
					{
						if( connection.To == Connection.From )
						// Backtracking. Skip.
						{
							continue;
						}
						
						properConnections.Add( connection );
					}
					
					return properConnections;
				}
			}

			public int CompareTo( object obj )
			{
				PathData other;
				
				other = obj as PathData;
				if( other != null )
				{
					return FScore.CompareTo( other.FScore );
				}
				else
				{
					throw new System.ArgumentException( "Invalid PathData given" );
				}
			}
		}
		
		
		
		private IEnumerator DoSeek()
		{
			ArrayList closedSet = new ArrayList(), openSetValues = new ArrayList();
			Hashtable openSet = new Hashtable();
			PathData currentPath = null;
			float endTime;
			
			endTime = Time.time + maxFrameTime;
			
			foreach( ConnectionAsset connection in Start.Connections )
			{
				if( ValidConnection( connection ) )
				{
					openSet[ connection ] = new PathData( connection, GScore( connection ), HScore( connection ) );
				}
			}
			
			while( seeking && openSet.Count > 0 )
			{
				openSetValues = new ArrayList( openSet.Values );
				openSetValues.Sort();
				currentPath = ( PathData )openSetValues[ 0 ];
				
				if( currentPath.Destination == End )
				{
					solution = currentPath.Path;
					break;
				}
				
				openSet.Remove( currentPath.Connection );
				closedSet.Add( currentPath.Connection );
				
				foreach( ConnectionAsset connection in currentPath.Connections )
				{
					if( closedSet.Contains( connection ) || !ValidConnection( connection ) )
					{
						continue;
					}
					
					if( !openSet.Contains( connection ) )
					{
						openSet[ connection ] = new PathData( currentPath, connection, GScore( connection ), HScore( connection ) );
					}
				}
				
				if( Time.time >= endTime )
				{
					yield return 0;
					endTime = Time.time + maxFrameTime;
				}
			}
		}
		
		
		
		public IEnumerator Seek()
		{
			ArrayList cache;
			
			Control.Instance.SeekerStarted( this );
			
			if( seeking )
			// Not nao!
			{
				Debug.LogError( "Seeker is busy" );
			}
			else if( Start == null || End == null )
			// I can't use these two together!
			{
				Debug.LogError( "Start and/or End is null" );
				
				solution.Clear();
				OnSearchFailed();
			}
			else if( Start == End )
			// No path to be found - we're done!
			{
				solution.Clear();
				OnSearchCompleted();
			}
			else
			// Seems like we need to do some work...
			{
				cache = Control.Instance.GetCache( this );
				
				if( cache != null )
				// We found a matching cached path!
				{
					solution = new ArrayList( cache );
					CacheLifespan = Control.NoCache;
					OnSearchCompleted();
				}
				else
				// No more beating around the bush. Pathfinding is needed!
				{
					solution.Clear();
				
					seeking = true;
				
					yield return Control.Instance.Owner.StartCoroutine( DoSeek() );
				
					if( solution.Count > 0 )
					// Wohoo!
					{
						OnSearchCompleted();
					}
					else
					// Aww...
					{
						OnSearchFailed();
					}
				
					seeking = false;
				}
			}
		}
		
		
		
		public void Stop()
		{
			seeking = false;
		}
		
		
		
		public void Kill()
		{
			Control.Instance.SeekerKilled( this );
			monitors.Clear();
		}
		
		
		
		public void Invalidate()
		{
			Stop();
			for( int i = 0; i < monitors.Count; i++ )
			{
				( ( ISearchMonitor )monitors[ i ] ).OnSeekerInvalidated( this );
			}
		}
		
		
		
		private void OnSearchCompleted()
		{
			Control.Instance.OnSearchCompleted( this );
			for( int i = 0; i < monitors.Count; i++ )
			{
				( ( ISearchMonitor )monitors[ i ] ).OnSearchCompleted( this );
			}
		}
		
		
		
		private void OnSearchFailed()
		{
			Control.Instance.OnSearchFailed( this );
			for( int i = 0; i < monitors.Count; i++ )
			{
				( ( ISearchMonitor )monitors[ i ] ).OnSearchFailed( this );
			}
		}
		
		
		
		public bool ValidPath( ArrayList path )
		{
			foreach( ConnectionAsset connection in path )
			{
				if( !ValidConnection( connection ) )
				{
					return false;
				}
			}
			
			return true;
		}
		
		
		
		public bool Valid( TaggedAsset asset )
		{
			foreach( string tag in requiredTags )
			{
				if( !asset.HasTag( tag ) )
				{
					return false;
				}
			}
			
			foreach( string tag in excludedTags )
			{
				if( asset.HasTag( tag ) )
				{
					return false;
				}
			}
			
			return true;
		}



		public bool ValidConnection( ConnectionAsset connection )
		{
			if( !connection.Enabled ||								// Connection is disabled?
				!connection.To.Enabled ||							// Target is disabled?
				connection.Width < radius * 2.0f ||					// Seeker doesn't fit?
				!Valid( connection ) ||								// Tags of connection don't fit our seeker?
				!Valid( connection.To )								// Tags of target don't fit our seeker?
			)
			{
				return false;
			}
			
			return true;
		}
	}
}
