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

// TODO: Go through all Add and Remove methods and prettify to the max

namespace PathLibrary
{
	public interface IPathAsset
	{
		byte[] Data
		{
			get;
			set;
		}
	}
	
	
	[ Serializable() ]
	public class CollectionAsset : ISerializable
	{
		private IPathAsset pathAsset;
		private NetworkAsset[] networks;
		private string[] tags;
		
		
		
		public CollectionAsset()
		{
			networks = new NetworkAsset[ 0 ];
			tags = new string[ 0 ];
		}
		
		
		
		public void Init( IPathAsset pathAsset )
		{
			this.pathAsset = pathAsset;
		}
		
		
		
		public CollectionAsset( SerializationInfo serializationInfo, StreamingContext streamingContext )
		{
			this.networks = serializationInfo.GetValue( "networks", typeof( NetworkAsset[] ) ) as NetworkAsset[];
			this.tags = serializationInfo.GetValue( "tags", typeof( string[] ) ) as string[];
		}
		
		
		
		public void GetObjectData( SerializationInfo serializationInfo, StreamingContext streamingContext )
		{
			serializationInfo.AddValue( "networks", this.networks );
			serializationInfo.AddValue( "tags", this.tags );
		}
		
		
		
		public byte[] GetData()
		{
			MemoryStream memoryStream;
			BinaryFormatter binaryFormatter;
		
			memoryStream = new MemoryStream();
			binaryFormatter = new BinaryFormatter();
		
			binaryFormatter.Serialize( memoryStream, this );
			memoryStream.Close();
		
			return memoryStream.ToArray();
		}
		
		
		
		public static CollectionAsset LoadFromData( byte[] data, IPathAsset pathAsset )
		{
			CollectionAsset collectionAsset;
			MemoryStream memoryStream;
			BinaryFormatter binaryFormatter;
			
			memoryStream = new MemoryStream( data );
			binaryFormatter = new BinaryFormatter();
		
			collectionAsset = binaryFormatter.Deserialize( memoryStream ) as CollectionAsset;
			collectionAsset.Init( pathAsset );
			
			return collectionAsset;
		}
		
		
		
		public void Save()
		{
			Editor.Instance.SaveCollection( this, pathAsset );
		}
		
		
		
		public NetworkAsset[] Networks
		{
			get
			{
				return networks;
			}
		}
		
		
		
		public NetworkAsset Get( string name )
		{
			foreach( NetworkAsset asset in networks )
			{
				if( asset.Name == name )
				{
					return asset;
				}
			}
			
			return null;
		}
		

		
		public NetworkAsset Add( NetworkAsset network )
		{
			NetworkAsset[] newNetworks;
			newNetworks = new NetworkAsset[ networks.Length + 1 ];
			
			for( int i = 0; i < networks.Length; i++ )
			{
				newNetworks[ i ] = networks[ i ];
			}
			
			newNetworks[ networks.Length ] = network;
			networks = newNetworks;
			
			return network;
		}
		
		
		
		public bool Remove( NetworkAsset network )
		{
			NetworkAsset[] newNetworks;
			bool found;
			
			newNetworks = new NetworkAsset[ networks.Length - 1 ];
			found = false;
			
			for( int i = 0; i < newNetworks.Length; i++ )
			{
				if( networks[ i ] == network )
				{
					found = true;
				}

				newNetworks[ i ] = networks[ i + ( ( found ) ? 1 : 0 ) ];
			}
			
			found = ( networks[ newNetworks.Length ] == network ) ? true : found;
			
			if( !found )
			{
				return false;
			}
			
			networks = newNetworks;

			return true;
		}
		
		
		
		public string[] Tags
		{
			get
			{
				return tags;
			}
		}
		
		
		
		public string AddTag( string tag )
		{
			string[] newTags;
			
			if( Array.IndexOf( tags, tag ) != -1 )
			{
				return tag;
			}
			
			newTags = new string[ tags.Length + 1 ];
			Array.Copy( tags, newTags, tags.Length );
			newTags[ tags.Length ] = tag;
			
			tags = newTags;
			
			return tag;
		}
		
		
		
		public bool RemoveTag( string tag )
		{
			int tagPosition;
			
			tagPosition = Array.IndexOf( tags, tag );
			
			if( tagPosition == -1 )
			{
				return false;
			}
			
			tags[ tagPosition ] = null;
				// TODO: We are doing this to preserve indexes. Find a cleaner way. Handling nulls in the list gives very ugly workarounds in the system.
			
			foreach( NetworkAsset network in Networks )
			{
				network.RemoveTag( tagPosition );
				
				foreach( NetworkNodeAsset node in network.Nodes )
				{
					node.RemoveTag( tagPosition );
					
					foreach( ConnectionAsset connection in node.Connections )
					{
						connection.RemoveTag( tagPosition );
					}
				}
			}

			return true;
		}
		
		
		
		public void OnRenderGizmos( Transform transform, float standardMarkerSize )
		{
			foreach( NetworkAsset network in Networks )
			{
				network.OnRenderGizmos( transform, standardMarkerSize );
			}
			
			if( Editor.Instance != null && Editor.Instance.SelectedNetwork is GridNetworkAsset )
			// Re-render selected grid network
			{
				Editor.Instance.SelectedNetwork.OnRenderGizmos( transform, standardMarkerSize );
			}
		}
	}
	
	
	[ Serializable() ]
	public class TaggedAsset
	{
		private CollectionAsset collection;
		private int[] tags;
		
		
		
		public TaggedAsset( CollectionAsset collection )
		{
			this.collection = collection;
			tags = new int[ 0 ];
		}
		
		
		
		public CollectionAsset Collection
		{
			get
			{
				return collection;
			}
		}
		
		
		
		public int[] Tags
		{
			get
			{
				return tags;
			}
			set
			{
				tags = value;
			}
		}
		
		
		
		public string[] TagStrings
		{
			get
			{
				string[] strings;
				
				strings = new string[ tags.Length ];
				
				for( int i = 0; i < tags.Length; i++ )
				{
					strings[ i ] = collection.Tags[ tags[ i ] ];
				}
				
				return strings;
			}
			set
			{
				int[] newTags;
				
				newTags = new int[ value.Length ];
				
				for( int i = 0; i < value.Length; i++ )
				{
					newTags[ i ] = Array.IndexOf( collection.Tags, value[ i ] );
					if( newTags[ i ] == -1 )
					{
						return;
					}
				}
				
				tags = newTags;
			}
		}
		
		
		
		public bool HasTag( int tag )
		{
			return Array.IndexOf( tags, tag ) != -1;
		}
		
		
		
		public bool HasTag( string tag )
		{
			return HasTag( Array.IndexOf( collection.Tags, tag ) );
		}
		
		
		
		public bool AddTag( int tag )
		{
			int[] newTags;
			
			if( HasTag( tag ) )
			{
				return false;
			}
			
			newTags = new int[ tags.Length + 1 ];
			Array.Copy( tags, newTags, tags.Length );
			newTags[ tags.Length ] = tag;
			
			tags = newTags;
			
			return true;
		}
		
		
		
		public bool AddTag( string tag )
		{
			return AddTag( Array.IndexOf( collection.Tags, tag ) );
		}
		
		
		
		public bool RemoveTag( int tag )
		{
			int[] newTags;
			int tagPosition;
			
			tagPosition = Array.IndexOf( tags, tag );
			
			if( tagPosition == -1 )
			{
				return false;
			}
			
			newTags = new int[ tags.Length - 1 ];
			
			Array.Copy( tags, newTags, tagPosition );
			Array.Copy( tags, tagPosition + 1, newTags, tagPosition, newTags.Length - tagPosition );
			
			tags = newTags;
			
			return true;
		}
		
		
		
		public bool RemoveTag( string tag )
		{
			return RemoveTag( Array.IndexOf( collection.Tags, tag ) );
		}
	}

}
