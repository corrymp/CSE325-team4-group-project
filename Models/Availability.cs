using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace Plan2Gather.Models;

/**
 * Availability is defined by a user, an event, and a set of times the user is available for
 * For the table, we use a joint primary key with the user and event IDs
 * The times are stored in a highly compressed format as explained below.
 *
 * The uncompressed data is a string of numbers where each number represents the status of a 
 * specific time: 0 for undeclared, 1 for available, 2 for maybe avaiable, and 3 for unavailable.
 * 
 * Compression involves splitting the string into 15 character chunks (padding the first chunk 
 * with 0s as needed), then parsing each chunk as a base 4 number.
 * 
 * The base 4 chunks are then converted to strings in base 36, and joined with '|' characters.
 * A '#' character is appended to the end followed by the number of 0s needed to pad the original first chunk.
 * 
 * To decompress the data, the compressed string is split on the '#' character, 
 * and the leading segement is split again on the '|' character.
 * 
 * Each chunk is then parsed to a base 36 number, converted to a string in base 4, 
 * and is padded with 0s to 15 characters. The chunks are then joined with no seperator.
 * 
 * If the portion of the compressed string after the '#' does not parse to 0, 
 * that many characters are removed from the head of the resulting string.
 * 
 * Both these algorythims were originaly written in JavaScript as follows:
 * ```js
 * const compress=a=>a&&(a.length%15?(' '.repeat(15-(a.length%15))+a):a).match(/.{1,15}/g).map(n=>parseInt(n,4).toString(36)).join('|')+'#'+(a.length%15);
 * 
 * const decompress=a=>(a=a.split('#'),a[0].split('|').map(n=>parseInt(n,36).toString(4).padStart(15,0)).join('').slice(15-(parseInt(a[1])||15)));
 * ```
 */

[PrimaryKey(nameof(UserId), nameof(EventId))]
public class Availability
{
    [Required, Key] public int UserId { get; set; }
    [Required, Key] public int EventId { get; set; }
    // RegExp to ensure the times is in the compressed format. Example: `h|53g7hx|ciph2o|cw169m|efg13r|79440a|15m2mc|2vacm3#3`
    [Required, RegularExpression("^([a-z0-9]{1,6}[|#])+[0-9]+$")] public required string Times { get; set; }
}
