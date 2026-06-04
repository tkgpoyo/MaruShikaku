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
                displayData.TerrainCells.Select(terrain => ToTerrainSaveData(terrain)).ToList(),
                displayData.StageObjects.Select(stageObject => ToObjectSaveData(stageObject)).ToList()
            );
        }

        public static StageDisplayData FromStageSaveData(StageSaveData saveData)
        {
            return new StageDisplayData() {
                MaruStart = saveData.MaruInitPos,
                SikakuStart = saveData.SikakuInitPos,
                Size = saveData.Size,
                TerrainCells = new(saveData.TerrainCells.Select(terrain => FromTerrainSaveData(terrain))),
                StageObjects = new(saveData.StageObjects.Select(stageObject => FromObjectSaveData(stageObject)))
            };
        }

        private static StageTerrainSaveData ToTerrainSaveData(StageTerrainCell terrainCell)
        {
            return new(terrainCell.Pos, terrainCell.Type);
        }

        private static StageTerrainCell FromTerrainSaveData(StageTerrainSaveData terrainSaveData)
        {
            return new(terrainSaveData.Pos, terrainSaveData.Type);
        }

        private static StageObjectSaveData ToObjectSaveData(StageObject stageObject)
        {
            if (stageObject is WallObject wallObject)
            {
                return new(
                    wallObject.Id,
                    wallObject.Pos,
                    wallObject.Type,
                    wallObject.SwitchId
                );
            }
            else
            {
                return new(
                    stageObject.Id,
                    stageObject.Pos,
                    stageObject.Type,
                    -1
                );
            }
        }

        private static StageObject FromObjectSaveData(StageObjectSaveData objectSaveData)
        {
            switch (objectSaveData.Type)
            {
                case EStageObjectType.Fragile:
                    return new FragileObject(
                        objectSaveData.Id,
                        objectSaveData.Pos
                    );
                case EStageObjectType.Movable:
                    return new MovableObject(
                        objectSaveData.Id,
                        objectSaveData.Pos
                    );
                case EStageObjectType.Spring:
                    return new SpringObject(
                        objectSaveData.Id,
                        objectSaveData.Pos
                    );
                case EStageObjectType.Switch:
                    return new SwitchObject(
                        objectSaveData.Id,
                        objectSaveData.Pos
                    );
                case EStageObjectType.Wall:
                    return new WallObject(
                        objectSaveData.Id,
                        objectSaveData.Pos,
                        objectSaveData.SwitchId
                    );
                default:
                    throw new NotImplementedException($"型{objectSaveData.Type}は実装していません．");
            }
        }
    }
}