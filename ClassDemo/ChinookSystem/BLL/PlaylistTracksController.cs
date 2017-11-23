using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Additional Namespaces
using Chinook.Data.Entities;
using Chinook.Data.DTOs;
using Chinook.Data.POCOs;
using ChinookSystem.DAL;
using System.ComponentModel;
#endregion

namespace ChinookSystem.BLL
{
    public class PlaylistTracksController
    {
        public List<UserPlaylistTrack> List_TracksForPlaylist(
            string playlistname, string username)
        {
            using (var context = new ChinookContext())
            {
                //what would happen if there is no match for the
                //  incoming parameter values.
                //we need to ensure that the results have a valid value
                //this value will the resolve of a query either a null (not found_
                //    or an IEnumerable<T> collection
                //to achieve a valid value encapsulate the query in a
                //   .FirstOrDefault()
                var results = (from x in context.Playlists
                              where x.UserName.Equals(username)
                                && x.Name.Equals(playlistname)
                              select x).FirstOrDefault();
               //test if you should return a null as your collection
               //  or find the tracks for the given PlaylistId in results.
                if (results == null)
                {
                    return null;
                }
                else
                {
                    //now get the tracks
                    var theTracks = from x in context.PlaylistTracks
                                where x.PlaylistId.Equals(results.PlaylistId)
                                orderby x.TrackNumber
                                select new UserPlaylistTrack
                                {
                                    TrackID = x.TrackId,
                                    TrackNumber = x.TrackNumber,
                                    TrackName = x.Track.Name,
                                    Milliseconds = x.Track.Milliseconds,
                                    UnitPrice = x.Track.UnitPrice
                                };
                    return theTracks.ToList();
                }
            }
        }//eom
        public List<UserPlaylistTrack> Add_TrackToPLaylist(string playlistname, string username, int trackid)
        {
            using (var context = new ChinookContext())
            {
                //code to go here
                //Part One:
                //query to get the playlist id
                var exists = (from x in context.Playlists
                               where x.UserName.Equals(username)
                                 && x.Name.Equals(playlistname)
                               select x).FirstOrDefault();

                //initialize the tracknumber
                int tracknumber = 0;
                //I will need to create an instance of PlaylistTrack
                PlaylistTrack newtrack = null;

                //determine if a playlist "parent" instances needs to be
                // created
                if (exists == null)
                {
                    //this is a new playlist
                    //create an instance of playlist to add to Playlist tablge
                    exists = new Playlist();
                    exists.Name = playlistname;
                    exists.UserName = username;
                    exists = context.Playlists.Add(exists);
                    //at this time there is NO phyiscal pkey
                    //the psuedo pkey is handled by the HashSet
                    tracknumber = 1;
                }
                else
                {
                    //playlist exists
                    //I need to generate the next track number
                    tracknumber = exists.PlaylistTracks.Count() + 1;

                    //validation: in our example a track can ONLY exist once
                    //   on a particular playlist
                    newtrack = exists.PlaylistTracks.SingleOrDefault(x => x.TrackId == trackid);
                    if (newtrack != null)
                    {
                        throw new Exception("Playlist already has requested track.");
                    }
                }

                //Part Two: Add the PlaylistTrack instance
                //use navigation to .Add the new track to PlaylistTrack
                newtrack = new PlaylistTrack();
                newtrack.TrackId = trackid;
                newtrack.TrackNumber = tracknumber;

                //NOTE: the pkey for PlaylistId may not yet exist
                //   using navigation one can let HashSet handle the PlaylistId
                //   pkey value
                exists.PlaylistTracks.Add(newtrack);

                //physically add all data to the database
                //commit
                context.SaveChanges();
                return List_TracksForPlaylist(playlistname, username);
            }
        }//eom
        public void MoveTrack(string username, string playlistname, int trackid, int tracknumber, string direction)
        {
            using (var context = new ChinookContext())
            {
                //code to go here 
                var exists = (from x in context.Playlists
                              where x.UserName.Equals(username)
                                && x.Name.Equals(playlistname)
                              select x).FirstOrDefault();
                if (exists == null)
                {
                    throw new Exception("Play list has been removed from the file.");
                }
                else
                {
                    PlaylistTrack moveTrack = (from x in exists.PlaylistTracks
                                               where x.TrackId == trackid
                                               select x).FirstOrDefault();
                    if (moveTrack == null)
                    {
                        throw new Exception("Play list track has been removed from the file.");
                    }
                    else
                    {
                        PlaylistTrack otherTrack = null;
                        if (direction.Equals("up"))
                        {
                            //up
                            if (moveTrack.TrackNumber == 1)
                            {
                                throw new Exception("Play list track already at top.");
                            }
                            else
                            {
                                otherTrack = (from x in exists.PlaylistTracks
                                              where x.TrackNumber == moveTrack.TrackNumber - 1
                                              select x).FirstOrDefault();
                                if (otherTrack == null)
                                {
                                    throw new Exception("Other Play list track is missing.");
                                }
                                else
                                {
                                    moveTrack.TrackNumber -= 1;
                                    otherTrack.TrackNumber += 1;
                                }
                            }
                        }
                        else
                        {
                            //down
                            if (moveTrack.TrackNumber == exists.PlaylistTracks.Count)
                            {
                                throw new Exception("Play list track already at bottom.");
                            }
                            else
                            {
                                otherTrack = (from x in exists.PlaylistTracks
                                              where x.TrackNumber == moveTrack.TrackNumber + 1
                                              select x).FirstOrDefault();
                                if (otherTrack == null)
                                {
                                    throw new Exception("Other Play list track is missing.");
                                }
                                else
                                {
                                    moveTrack.TrackNumber += 1;
                                    otherTrack.TrackNumber -= 1;
                                }
                            }
                        }//eof up/down
                        //staging
                        context.Entry(moveTrack).Property(y => y.TrackNumber).IsModified = true;
                        context.Entry(otherTrack).Property(y => y.TrackNumber).IsModified = true;
                        //saving (apply update to database
                        context.SaveChanges();
                    }
                }
            }
        }//eom


        public void DeleteTracks(string username, string playlistname, List<int> trackstodelete)
        {
            using (var context = new ChinookContext())
            {
                //code to go here
                var exists = (from x in context.Playlists
                              where x.UserName.Equals(username)
                                && x.Name.Equals(playlistname)
                              select x).FirstOrDefault();
                if (exists == null)
                {
                    throw new Exception("Play list has been removed from the file.");
                }
                else
                {
                    //find tracks that will be kept
                    var tracksKept = exists.PlaylistTracks
                                     .Where(tr => !trackstodelete.Any(tod => tod == tr.TrackId))
                                     .Select(tr => tr);

                    //remove unwanted tracks
                    PlaylistTrack item = null;
                    foreach(var dtrackid in trackstodelete)
                    {
                        item = exists.PlaylistTracks
                            .Where(tr => tr.TrackId == dtrackid)
                            .FirstOrDefault();
                        if (item != null)
                        {
                            exists.PlaylistTracks.Remove(item);
                        }
                        
                    }

                    //renumber remaining (Kept) list
                    int number = 1;
                    foreach(var tKept in tracksKept)
                    {
                        tKept.TrackNumber = number;
                        context.Entry(tKept).Property(y => y.TrackNumber).IsModified = true;
                        number++;
                    }

                    context.SaveChanges();
                }

            }
        }//eom
    }
}
