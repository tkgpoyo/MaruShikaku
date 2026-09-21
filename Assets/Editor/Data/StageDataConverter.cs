using System;
using System.Linq;
using MaruSikaku.Stage;

namespace MaruSikaku.Editor.Data
{
    public static class StageDataConverter
    {
        public static StageSaveData ToStageSaveData(StageDisplayData displayData)
        {
            return new(
                displayData.Size,
                displayData.MaruStart,
                displayData.SikakuStart,
                displayData.MaruGoal,
                displayData.SikakuGoal,
                displayData.TerrainCells.Select(terrain => ToTerrainSaveData(terrain)).ToList(),
                displayData.StageObjects.Select(stageObject => ToObjectSaveData(stageObject)).ToList()
            );
        }

        public static StageDisplayData FromStageSaveData(StageSaveData saveData)
        {
            var data = new StageDisplayData() {
                MaruStart = saveData.MaruInitPos,
                SikakuStart = saveData.SikakuInitPos,
                MaruGoal = saveData.MaruGoalPos,
                SikakuGoal = saveData.SikakuGoalPos,
                Size = saveData.Size,
            };
            data.SetTerrainCells(saveData.TerrainCells.Select(terrain => FromTerrainSaveData(terrain)));
            data.SetStageObjects(saveData.StageObjects.Select(stageObject => FromObjectSaveData(stageObject)));
            return data;
        }

        private static StageTerrainSaveData ToTerrainSaveData(StageTerrainCell terrainCell)
        {
            return new(terrainCell.Pos, terrainCell.Type);
        }

        private static StageTerrainCell FromTerrainSaveData(StageTerrainSaveData terrainSaveData)
        {
            return new(terrainSaveData.Pos, terrainSaveData.Type);
        }

        private static StageObjectSaveData ToObjectSaveData(StageObjectDisplayData stageObject)
        {
            if (stageObject is WallObjectDisplayData wallObject)
            {
                return new(
                    wallObject.Id,
                    wallObject.Pos,
                    wallObject.Type,
                    wallObject.SwitchId,
                    wallObject.YLength
                );
            }
            else
            {
                return new(
                    stageObject.Id,
                    stageObject.Pos,
                    stageObject.Type,
                    -1,
                    -1
                );
            }
        }

        private static StageObjectDisplayData FromObjectSaveData(StageObjectSaveData objectSaveData)
        {
            switch (objectSaveData.Type)
            {
                case EStageObjectType.Fragile:
                    return new FragileObjectDisplayData(
                        objectSaveData.Id,
                        objectSaveData.Pos
                    );
                case EStageObjectType.Movable:
                    return new MovableObjectDisplayData(
                        objectSaveData.Id,
                        objectSaveData.Pos
                    );
                case EStageObjectType.Spring:
                    return new SpringObjectDisplayData(
                        objectSaveData.Id,
                        objectSaveData.Pos
                    );
                case EStageObjectType.Switch:
                    return new SwitchObjectDisplayData(
                        objectSaveData.Id,
                        objectSaveData.Pos
                    );
                case EStageObjectType.Wall:
                    return new WallObjectDisplayData(
                        objectSaveData.Id,
                        objectSaveData.Pos,
                        objectSaveData.SwitchId,
                        objectSaveData.YLength
                    );
                default:
                    throw new NotImplementedException($"型{objectSaveData.Type}は実装していません．");
            }
        }
    }
}