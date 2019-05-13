using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeOperator : MonoBehaviour {
  public int size_X = 10;
  public int size_Y = 10;
  public float road_size = 1;
  public float wall_size = 0.2f;
  public int wait_time = 500;

  public GameObject wall;
  public GameObject fulcrum;

  struct Block {
    public int status;
    public int group;
    public int enables;
  }

  struct Position {
    public int X;
    public int Y;
  }

  enum Direction { T, R, B, L }

  Dictionary<Direction, int> DirectionNumber = new Dictionary<Direction, int>() {
    {Direction.T, 1},
    {Direction.L, 2},
    {Direction.B, 4},
    {Direction.R, 8}
  };

  Dictionary<string, Block> _blocks = new Dictionary<string, Block>();
  Dictionary<int, List<string>> _groups = new Dictionary<int, List<string>>() { { 0, new List<string>() } };
  Dictionary<int, int> _group_belongs = new Dictionary<int, int>() { { 0, 0 } };
  string _block_id = "";
  Direction? _direction = null;
  int _group_id = 0;
  List<string> _rest_block_ids = new List<string>();

  void Start() {
    Vector3 pos = new Vector3(0, 0, 0);

    for (int Y = 0; Y <= this.size_Y; Y++) {
      int enables_Y = ((Y > 0) ? this.DirectionNumber[Direction.T] : 0) + ((Y < this.size_Y - 1) ? this.DirectionNumber[Direction.B] : 0);

      for (int X = 0; X <= this.size_X; X++) {
        if (Y < this.size_Y) {
          GameObject wall_copy_col = Instantiate(this.wall, new Vector3(
            pos.x + ((this.road_size + this.wall_size) * X),
            pos.y,
            pos.z + ((this.road_size + this.wall_size) * Y) + ((this.wall_size + this.road_size) / 2)
          ), new Quaternion(0, 90, 0, 90));

          wall_copy_col.name = this._convertPositionToId(Y, X) + Direction.L;
        }

        if (X < this.size_X) {
          GameObject wall_copy_row = Instantiate(this.wall, new Vector3(
            pos.x + ((this.road_size + this.wall_size) * X) + ((this.wall_size + this.road_size) / 2),
            pos.y,
            pos.z + ((this.road_size + this.wall_size) * Y)
          ), Quaternion.identity);

          wall_copy_row.name = this._convertPositionToId(Y, X) + Direction.T;
        }

        GameObject fulcrum_copy = Instantiate(this.fulcrum, new Vector3(
          pos.x + ((this.road_size + this.wall_size) * X),
          pos.y,
          pos.z + ((this.road_size + this.wall_size) * Y)
        ), Quaternion.identity);

        if ((Y == this.size_Y) || (X == this.size_X)) continue;

        int enables = enables_Y + ((X > 0) ? this.DirectionNumber[Direction.L] : 0) + ((X < this.size_X - 1) ? this.DirectionNumber[Direction.R] : 0);

        string block_id = this._convertPositionToId(Y, X);

        Block block = new Block();
        block.status = 0;
        block.group = this._group_id;
        block.enables = enables;

        this._blocks[block_id] = block;
        this._rest_block_ids.Add(block_id);
        this._groups[this._group_id].Add(block_id);
      }
    }

    this.Make();
  }

  void Make() {
    this._connectBlock();

    if (this._rest_block_ids.Count() > 0) {
      new WaitForSeconds(this.wait_time);

      this.Make();
    }
  }

  void _connectBlock() {
    if (this._block_id == "") {
      this._pickBlock();

      return;
    }

    bool result = false;

    Block block = this._blocks[this._block_id];

    Direction? reverse_direction = null;

    if (this._direction.HasValue) this._getBackDirection(this._direction.Value);

    foreach (Direction direction in this._getBackDirections(true)) {
      if (direction == reverse_direction) continue;

      string next_block_id = this._shiftId(this._block_id, direction);

      if (next_block_id != "") {
        Block next_block = this._blocks[next_block_id];

        if ((this._group_belongs[next_block.group] == 0) || (this._group_belongs[next_block.group] != this._group_belongs[block.group])) {
          result = true;

          string target_block_id = this._block_id;
          Direction target_direction = direction;

          this._block_id = "";
          this._direction = direction;

          switch (direction) {
            case Direction.R:
            case Direction.B:
              target_block_id = next_block_id;
              target_direction = this._getBackDirection(direction);
              break;
          }

          Block target_block = this._blocks[target_block_id];

          target_block.status += this.DirectionNumber[target_direction];

          this._blocks[target_block_id] = target_block;

          GameObject wall = GameObject.Find(target_block_id + target_direction);

          GameObject.Destroy(wall);

          if (next_block.group == 0) {
            next_block.group = block.group;

            this._blocks[next_block_id] = next_block;
            this._block_id = next_block_id;

            int next_block_group_number = this._groups[0].IndexOf(next_block_id);

            this._groups[0].RemoveAt(next_block_group_number);
            this._groups[this._group_belongs[block.group]].Add(next_block_id);
          } else {
            this._mergeGroup(block.group, next_block.group);
          }

          if (!this._checkBlockEnables(next_block_id, this._group_belongs[next_block.group])) {
            this._block_id = "";
          }

          break;
        }
      }

      if (this._direction is Direction) break;
    }

    if (!result) this._pickBlock();
  }

  void _pickBlock() {
    this._direction = null;

    int rand_number = new System.Random().Next(0, this._rest_block_ids.Count() - 1);

    this._block_id = this._rest_block_ids[rand_number];

    Block block = this._blocks[this._block_id];

    if (block.group != 0) {
      this._connectBlock();

      return;
    }

    this._group_id++;
    this._groups[this._group_id] = new List<string>() { this._block_id };
    this._group_belongs[this._group_id] = this._group_id;
    block.group = this._group_id;

    this._blocks[this._block_id] = block;

    int block_group_number = this._groups[0].IndexOf(this._block_id);
    if (block_group_number >= 0) this._groups[0].RemoveAt(block_group_number);
  }

  void _mergeGroup(int group_1, int group_2) {
    int group_1_base = this._group_belongs[group_1];
    int group_2_base = this._group_belongs[group_2];

    this._groups[group_1_base] = this._groups[group_1_base]
      .Where((string block_id) => this._checkBlockEnables(block_id, group_2_base, false))
      .Concat(this._groups[group_2_base])
      .ToList();

    for (int group = 1; group <= this._group_id; group++) {
      if (this._group_belongs[group] == group_2_base) {
        this._group_belongs[group] = group_1_base;
      }
    }

    this._groups.Remove(group_2_base);
  }

  bool _checkBlockEnables(string block_id, int group, bool delete_group = true) {
    foreach (Direction direction in this._getBackDirections()) {
      string next_block_id = this._shiftId(block_id, direction);
      if (next_block_id == "") continue;

      Block next_block = this._blocks[next_block_id];

      if (this._group_belongs[next_block.group] == group) {
        this._disableBlockDirection(block_id, direction, delete_group);
        this._disableBlockDirection(next_block_id, this._getBackDirection(direction));
      }
    }

    return this._blocks[block_id].enables > 0;
  }

  void _disableBlockDirection(string block_id, Direction direction, bool delete_group = true) {
    Block block = this._blocks[block_id];
    int block_group_base = this._group_belongs[block.group];
    int direction_number = DirectionNumber[direction];

    if ((direction_number & block.enables) > 0) {
      block.enables -= direction_number;

      this._blocks[block_id] = block;

      if (block.enables == 0) {
        int stack_number = this._rest_block_ids.IndexOf(block_id);
        if (stack_number >= 0) this._rest_block_ids.RemoveAt(stack_number);

        if (delete_group) {
          int stack_group_number = this._groups[block_group_base].IndexOf(block_id);
          if (stack_group_number >= 0) this._groups[block_group_base].RemoveAt(stack_group_number);
        }
      }
    }
  }

  Position? _shiftPosition(Position position, Direction direction) {
    Position shifted_position;
    shifted_position.X = position.X;
    shifted_position.Y = position.Y;

    if (direction == Direction.T) {
      if (position.Y <= 0) return null; 
      shifted_position.Y -= 1;
    }
    if (direction == Direction.R) {
      if (position.X >= this.size_X - 1) return null; 
      shifted_position.X += 1;
    }
    if (direction == Direction.B) {
      if (position.Y >= this.size_X - 1) return null; 
      shifted_position.Y += 1;
    }
    if (direction == Direction.L) {
      if (position.X <= 0) return null; 
      shifted_position.X -= 1;
    }

    return shifted_position;
  }

  string _shiftId(string id, Direction direction) {
    Position position_pre = this._convertIdToPosition(id);
    Position? position_post = this._shiftPosition(position_pre, direction);

    return (position_post.HasValue)
      ? this._convertPositionToId(position_post.Value.Y, position_post.Value.X)
      : "";
  }

  List<Direction> _getBackDirections(bool random = false) {
    List<Direction> directions = new List<Direction>() {
      Direction.T,
      Direction.R,
      Direction.B,
      Direction.L
    };

    if (!random) return directions;

    System.Random randomizer = new System.Random();

Debug.Log(randomizer);
    return directions.OrderBy(direction => randomizer.Next(4)).ToList();
  }

  Direction _getBackDirection(Direction direction) {
    List<Direction> directions = this._getBackDirections();

    int direction_number = (directions.IndexOf(direction) + 2) % 4;

    return directions[direction_number];
  }

  string _convertPositionToId(int Y, int X) {
    return Y + "|" + X;
  }

  Position _convertIdToPosition(string id) {
    string[] poinsion = id.Split('|');

    Position position;
    position.Y = int.Parse(poinsion[0]);
    position.X = int.Parse(poinsion[1]);

    return position;
  }
}
