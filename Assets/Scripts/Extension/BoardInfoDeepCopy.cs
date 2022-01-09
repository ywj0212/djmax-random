using System.Collections.Generic;
using Mirix.DMRV;

public static class BoardInfoDeepCopy
{
    public static BoardInfo DeepCopy(this BoardInfo b) {
        BoardInfo output = new BoardInfo(b.Name, new Board());

        output.Board.TargetProgramVersion = b.Board.TargetProgramVersion;

        if(b.Board.Criteria != null) {
            output.Board.Criteria = new Board.CriteriaData();
            output.Board.Criteria.Crit = b.Board.Criteria.Crit;
            output.Board.Criteria.Rate = b.Board.Criteria.Rate;
        }
        else output.Board.Criteria = null;

        output.Board.Modifyable = true;
        output.Board.ButtonType = b.Board.ButtonType;
        output.Board.CategoryType = b.Board.CategoryType;

        List<Board.ButtonData> bd = new List<Board.ButtonData>();
        foreach(var btd in b.Board.Buttons) {
            Board.ButtonData t1 = new Board.ButtonData();

            t1.Lv = new List<Board.ButtonData.LvData>();
            foreach(var lvd in btd.Lv) {
                Board.ButtonData.LvData t2 = new Board.ButtonData.LvData();

                t2.Lv = lvd.Lv;
                t2.Floor = new List<Board.ButtonData.LvData.FloorData>();

                foreach(var fld in lvd.Floor) {
                    Board.ButtonData.LvData.FloorData t3 = new Board.ButtonData.LvData.FloorData();
                    t3.Name = fld.Name;

                    List<ushort> tl = new List<ushort>();
                    foreach(var trd in fld.Tracks) tl.Add(trd);
                    t3.Tracks = tl;

                    if(fld.Qualification != null) {
                        t3.Qualification = new Board.ButtonData.LvData.FloorData.QualificationData();

                        if(fld.Qualification.PlayOption != null) {
                            t3.Qualification.PlayOption = new Board.ButtonData.LvData.FloorData.PlayOptionData();
                            
                            t3.Qualification.PlayOption.Speed = fld.Qualification.PlayOption.Speed;
                            t3.Qualification.PlayOption.Fever = fld.Qualification.PlayOption.Fever;
                            t3.Qualification.PlayOption.Fader = fld.Qualification.PlayOption.Fader;
                            t3.Qualification.PlayOption.Chaos = fld.Qualification.PlayOption.Chaos;
                        }

                        t3.Qualification.QualificationTier = new List<Board.ButtonData.LvData.FloorData.QualificationTierData>();
                        foreach(var qt in fld.Qualification.QualificationTier) {
                            Board.ButtonData.LvData.FloorData.QualificationTierData tq = new Board.ButtonData.LvData.FloorData.QualificationTierData();
                            tq.Break = qt.Break;
                            tq.Rate = qt.Rate;
                            tq.Additional = qt.Additional;

                            t3.Qualification.QualificationTier.Add(tq);
                        }
                    }

                    t2.Floor.Add(t3);
                }

                t1.Lv.Add(t2);
            }

            bd.Add(t1);
        }

        output.Board.Buttons = bd.ToArray();

        return output;
    }
}
