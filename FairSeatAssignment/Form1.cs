using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Troschuetz.Random;

namespace FairSeatAssignment
{
    public partial class MainForm : Form
    {
        TRandom m_rng;                      // 난수 생성기
        CheckBox[] m_totalSeats;            // 전체 좌석
        List<CheckBox> m_seats;             // 유효 좌석
        //List<string> m_names;               // 전체 이름표
        string[] m_assigned;                // 좌석별 배정받은 이름표
        //int m_totalNames;                   // 전체 이름 수
        int m_cachedTotalSeats;             // 캐시된 유효 좌석 수

        bool m_seatsRead = false;           // 좌석 정보를 읽어온 경우
        bool m_namesRead = false;           // 이름표를 읽어온 경우

        bool m_fireAssign;                  // 배정 명령이 진행중인지 여부
        int m_curStep = CUR_STEP_INIT_VAL;  // 현재 배정할 좌석 번호
        bool m_continuous = false;          // 연속배정 여부

        int m_prioritySeats;                // 우선순위 좌석 수
        int m_normalSeats;                  // 일반 배정될 좌석 번호의 최대
        int m_lastSeats;                    // 후순위 배정될 좌석 번호의 최대
        List<string> m_priorityList;        // 우선순위 명부
        List<string> m_namesList;           // 일반 명부
        List<string> m_lastList;            // 후순위 좌석 수

        long tickClicked = -1L;             // 프로그램이 켜진 다음 흐른 시간 단위 ms

        const string FSA_NAMES = "fsanames.txt"; // 이름 정보가 저장된 파일 이름
        const string FSA_SEATS = "fsaseats.txt"; // 좌석정보가 저장된 파일 이름
        const int CUR_STEP_INIT_VAL = -1;   // 배정할 좌석번호가 0보다 작으면 초기화(미배정) 상태입니다.
        const string FSA_TAG = "FairSeatAssignment"; // FSA에서 관리하는 파일임을 나타내는 지시자 문자열
        const string FSA_FIRST_TAG = "---First---";  // 우선순위 이름표 지시자
        const string FSA_NAMES_TAG = "---Names---";  // 일반 이름표 지시자
        const string FSA_LAST_TAG = "---Last----";   // 후순위 이름표 지시자

        bool isUserChange = true; // SelectedIndexChanged 리스너에서 목록 선택 이벤트를 처리해야할지 말지를 결정

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Random 객체 초기화
            m_rng = new TRandom();

            // 자리 배열 초기화
            // 앞자리 왼쪽부터 좌선우후 순으로 자리를 배치함
            // UI 객체는 임의로 생성하지 않았으므로 이렇게 배열에 하나하나 삽입
            m_totalSeats = new CheckBox[50];
            m_totalSeats[0] = seat01;
            m_totalSeats[1] = seat02;
            m_totalSeats[2] = seat03;
            m_totalSeats[3] = seat04;
            m_totalSeats[4] = seat05;
            m_totalSeats[5] = seat06;
            m_totalSeats[6] = seat07;
            m_totalSeats[7] = seat08;
            m_totalSeats[8] = seat09;
            m_totalSeats[9] = seat10;
            m_totalSeats[10] = seat11;
            m_totalSeats[11] = seat12;
            m_totalSeats[12] = seat13;
            m_totalSeats[13] = seat14;
            m_totalSeats[14] = seat15;
            m_totalSeats[15] = seat16;
            m_totalSeats[16] = seat17;
            m_totalSeats[17] = seat18;
            m_totalSeats[18] = seat19;
            m_totalSeats[19] = seat20;
            m_totalSeats[20] = seat21;
            m_totalSeats[21] = seat22;
            m_totalSeats[22] = seat23;
            m_totalSeats[23] = seat24;
            m_totalSeats[24] = seat25;
            m_totalSeats[25] = seat26;
            m_totalSeats[26] = seat27;
            m_totalSeats[27] = seat28;
            m_totalSeats[28] = seat29;
            m_totalSeats[29] = seat30;
            m_totalSeats[30] = seat31;
            m_totalSeats[31] = seat32;
            m_totalSeats[32] = seat33;
            m_totalSeats[33] = seat34;
            m_totalSeats[34] = seat35;
            m_totalSeats[35] = seat36;
            m_totalSeats[36] = seat37;
            m_totalSeats[37] = seat38;
            m_totalSeats[38] = seat39;
            m_totalSeats[39] = seat40;
            m_totalSeats[40] = seat41;
            m_totalSeats[41] = seat42;
            m_totalSeats[42] = seat43;
            m_totalSeats[43] = seat44;
            m_totalSeats[44] = seat45;
            m_totalSeats[45] = seat46;
            m_totalSeats[46] = seat47;
            m_totalSeats[47] = seat48;
            m_totalSeats[48] = seat49;
            m_totalSeats[49] = seat50;

           

            string namesTxt = null;
            //m_names = new List<string>(); // names 도 일반 listBox에 통합합니다. 필요없음
            try
            {
                namesTxt = System.IO.File.ReadAllText(FSA_NAMES);
            }
            catch (System.IO.IOException)
            {
                // 파일 읽기 실패
                MessageBox.Show("입력 파일이 제공되지 않았습니다.\n이름을 추가/제거 하신 경우 이름 정보가 본 프로그램 종료시 "+ FSA_NAMES + "로 실행파일과 같은 폴더에 생성됩니다.");

                //// 번호모드로 동작
                //for (int i = 0; i < m_totalSeats.Length; i++)
                //{
                //    listName.Items.Add("" + (i + 1));
                //}
            }

            if (namesTxt != null)
            {
                // 파일을 읽어왔습니다.
                string[] splitNames = namesTxt.Split('\n'); // 줄 단위로 나누기

                if (FSA_TAG.Equals(splitNames[0].Trim()))
                {
                    m_namesRead = true;
                }

                if (m_namesRead)
                {
                    // 이름표를 읽어온 경우

                    short flag = 0; //1: 우선순위, 2: 일반, 3: 후순위

                    for (int i = 1; i < splitNames.Length; i++)
                    {
                        //트림한 문자열을 이름 목록에 추가
                        string name = splitNames[i].Trim();

                        if (FSA_FIRST_TAG.Equals(name))
                        {
                            // 우선순위 이름표 지시자를 만났습니다.

                            flag = 1;
                            continue;
                        }
                        else if (FSA_NAMES_TAG.Equals(name))
                        {
                            // 일반 이름표 지시자를 만났습니다.

                            flag = 2;
                            continue;
                        }
                        else if (FSA_LAST_TAG.Equals(name))
                        {
                            // 후순위 이름표 지시자를 만났습니다.

                            flag = 3;
                            continue;
                        }


                        if (!"".Equals(name))
                        {
                            // 빈 칸이 아닌경우 이름 목록에 추가
                            switch (flag)
                            {
                                case 1: // 우선순위 이름표
                                    listPriority.Items.Add(name);
                                    break;
                                default:
                                case 2: // 일반 이름표, 기본값
                                    listName.Items.Add(name);
                                    break;
                                case 3: // 후순위 이름표
                                    listLast.Items.Add(name);
                                    break;
                            }
                        }
                    }
                }
            }


            // 전체 이름 수;
            //m_totalNames = listPriority.Items.Count + listName.Items.Count + listLast.Items.Count;


            string seatsTxt = null;
            try
            {
                seatsTxt = System.IO.File.ReadAllText(FSA_SEATS);
            }
            catch (System.IO.IOException)
            {
                // 파일 읽기 실패
                MessageBox.Show("입력 파일이 제공되지 않아 좌석이 모두 선택되었습니다.\n체크를 건드리시면 본 프로그램 종료시 좌석정보가 "+ FSA_SEATS + "로 실행파일과 같은 폴더에 생성됩니다.");
            }

            if(seatsTxt != null)
            {
                // 0과 1의 문자열을 줄 단위로 분해해 trim하고 
                // 0이면 해제 1이면 체크 이렇게 설정합니다.
                string[] splittedSeats = seatsTxt.Split('\n');

                if (FSA_TAG.Equals(splittedSeats[0].Trim()))
                {
                    // 첫줄이 FairSeatAssignment 일 때만 읽어오도록 합니다.
                    m_seatsRead = true;
                }

                if (m_seatsRead)
                {
                    // 좌석정보를 읽을 수 있었던 경우

                    int seatChecker = 0;
                    for (int row = 1; row < splittedSeats.Length; row++)
                    {
                        string seats = splittedSeats[row].Trim();

                        for (int cur = 0; cur < seats.Length && seatChecker < m_totalSeats.Length; cur++)
                        {
                            if ('0'.Equals(seats.ElementAt(cur)))
                            {
                                m_totalSeats[seatChecker].Checked = false;
                            }
                            else
                            {
                                m_totalSeats[seatChecker].Checked = true;
                            }
                            seatChecker++;
                        }
                    }
                }
            }

            //체크된 좌석 수를 미리 세둡니다.
            getCheckedSeats();

            for (int seatNo = 0; seatNo < m_totalSeats.Length; seatNo++)
            {
                //체크 변경시 이벤트 추가
                m_totalSeats[seatNo].CheckedChanged += onSeatCheckChanged;
            }

            // 입력 이름 수가 최대 좌석 수보다 큰 경우 경고 표시
            if (getTotalNames() > m_seats.Count)
            {
                MessageBox.Show("입력 이름 수(" + getTotalNames() + ")가 가용 좌석 수(" + m_seats.Count + ")보다 많습니다.\n일부는 자리를 배정받지 못할 것입니다.");
            }
        }

        public void onSeatCheckChanged(object sender, EventArgs e)
        {
            // 체크박스 객체에서 체크가 되면 캐시된 좌석수에 +1, 풀리면 -1; 
            if(sender is CheckBox)
            {
                CheckBox ckb = (CheckBox)sender;
                if (ckb.Checked)
                {
                    m_cachedTotalSeats++;
                }
                else
                {
                    m_cachedTotalSeats--;
                }

                // 좌석 정보를 저장해야합니다.
                m_seatsRead = true;

                setLabelAvailability(m_cachedTotalSeats);
            }
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 좌석 배정 도중 창을 닫으려고 하는 경우 경고창을 띄웁니다.
            if(m_curStep >= 0) {
                MessageBox.Show("초기화 버튼을 눌러 배정된 값을 모두 지운 다음 종료하십시오.", "알립니다", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }

            if (m_seatsRead)
            {
                // 좌석 수를 읽어올 수 있었던 경우 저장합니다.
                try
                {
                    System.IO.File.WriteAllText(FSA_SEATS, getCheckedSeats());
                }
                catch (System.IO.IOException)
                {
                    // 파일 읽기 실패
                    MessageBox.Show("좌석 정보 저장에 실패하였습니다. 다음 실행시 기존에 저장된 좌석 정보를 이용합니다.");
                }
            
            }

            if (m_namesRead)
            {

                // 이름표를 읽어올 수 있었던 경우 저장합니다.
                try
                {
                    System.IO.File.WriteAllText(FSA_NAMES, getAssignedNames());
                }
                catch (System.IO.IOException)
                {
                    // 파일 읽기 실패
                    MessageBox.Show("이름정보 저장에 실패하였습니다.");
                }
            }
        }

        private string getAssignedNames()
        {
            //좌석 정보 문자열을 조립할 스트링 빌더
            StringBuilder sbNames = new StringBuilder();
            sbNames.Append(FSA_TAG + "\r\n");

            // 우선순위 이름표 저장
            sbNames.Append(FSA_FIRST_TAG + "\r\n");
            foreach (string priorityName in listPriority.Items)
            {
                sbNames.Append(priorityName + "\r\n");
            }

            // 일반 이름표 저장
            sbNames.Append(FSA_NAMES_TAG + "\r\n");
            foreach (string name in listName.Items)
            {
                sbNames.Append(name + "\r\n");
            }

            // 후순위 이름표 저장
            sbNames.Append(FSA_LAST_TAG + "\r\n");
            foreach (string lastName in listLast.Items)
            {
                sbNames.Append(lastName + "\r\n");
            }


            return sbNames.ToString();
        }

        private int getTotalNames()
        {
            return listName.Items.Count + listPriority.Items.Count + listLast.Items.Count;
        }

        private void setLabelAvailability(int availableSeats)
        {
            // 좌석 수 불일치시 캐시하기
            if(availableSeats != m_cachedTotalSeats)
            {
                m_cachedTotalSeats = availableSeats;
            }

            lblAvailability.Text = "가용 좌석 수: " + availableSeats + "\n입력 이름 수: " + getTotalNames();
            if(availableSeats < getTotalNames())
            {
                // 가용 좌석 수가 전체 이름 수보다 모자라면 빨강색으로 강조
                lblAvailability.ForeColor = Color.Red;
            }
            else
            {
                // 가용 좌석 수가 전체 이름 수보다 모자라면 검정색으로 돌아감
                lblAvailability.ForeColor = Color.Black;
            }
        }

        private void btnAssign_Click(object sender, EventArgs e)
        {
            //단계별 지정 버튼 비활성화
            btnAssignStepbyStep.Enabled = false;
            m_continuous = true;

            //배정 시작.
            btnAssignStepbyStep_Click(sender, e);
            /*
            m_assigned = new string[m_seats.Count];

            
            if (ckbConsolidate.Checked)
            {
                // 앞자리부터 순서대로 채우기 활성화


                // 우선순위 자리 채우기
                string[] priorityList = new string[listPriority.Items.Count];
                for (int i = 0; i < listPriority.Items.Count; i++)
                {
                    priorityList[i] = (string)listPriority.Items[i];
                }

                int proritySeats = priorityList.Length;
                if (proritySeats > m_seats.Count)
                {
                    // 좌석수보다 사람 수가 많은 경우
                    proritySeats = m_seats.Count;
                }

                for (int insertCur = 0; insertCur < priorityList.Length && insertCur < m_assigned.Length; insertCur++)
                {
                    if (m_assigned[insertCur] != null)
                    {
                        //이미 값이 채워진 경우 지나치도록 합니다.
                        continue;
                    }
                    // 우선순위 자리 채우기
                    int nextSeatPriority = m_rng.Next(0, priorityList.Length);
                    m_assigned[insertCur] = priorityList[nextSeatPriority];
                    for (int searchCur = 0; searchCur < insertCur; searchCur++)
                    {
                        if (m_assigned[searchCur].Equals(m_assigned[insertCur]))
                        {
                            // 검색중 중복 배정임이 확인되면 다시 하도록 한다.
                            m_assigned[insertCur] = null;
                            insertCur--;
                            break;
                        }
                    }
                }

                // 일반 좌석수는 우선순위 ~ (우선순위+ 이름 목록 수)
                int normalSeats = proritySeats + m_names.Count;
                if (normalSeats > m_seats.Count)
                {
                    // 좌석수보다 사람 수가 많은 경우
                    normalSeats = m_seats.Count;
                }

                // 일반 자리 채우기
                for (int insertCur = priorityList.Length; insertCur < normalSeats; insertCur++)
                {
                    if (m_assigned[insertCur] != null)
                    {
                        //이미 값이 채워진 경우 지나치도록 합니다.
                        continue;
                    }

                    // 0포함 m_names.Length제외 까지의 임의의 수
                    int nextSeat = m_rng.Next(0, m_names.Count);
                    m_assigned[insertCur] = m_names[nextSeat];
                    for (int searchCur = 0; searchCur < insertCur; searchCur++)
                    {
                        if (m_assigned[searchCur].Equals(m_assigned[insertCur]))
                        {
                            // 검색중 중복 배정임이 확인되면 다시 하도록 한다.
                            m_assigned[insertCur] = null;
                            insertCur--;
                            break;
                        }
                    }
                }

                // 후순위 자리 채우기
                string[] lastList = new string[listLast.Items.Count];
                for (int i = 0; i < listLast.Items.Count; i++)
                {
                    lastList[i] = (string)listLast.Items[i];
                }

                int totalSeats = normalSeats + lastList.Length;
                if (totalSeats > m_seats.Count)
                {
                    // 좌석수보다 사람 수가 많은 경우
                    totalSeats = m_seats.Count;
                }

                for (int insertCur = 0; insertCur < lastList.Length && insertCur < m_assigned.Length; insertCur++)
                {
                    if (m_assigned[insertCur] != null)
                    {
                        //이미 값이 채워진 경우 지나치도록 합니다.
                        continue;
                    }
                    // 후순위 자리 채우기
                    int nextSeatLast = m_rng.Next(0, lastList.Length);
                    m_assigned[insertCur] = lastList[nextSeatLast];
                    for (int searchCur = 0; searchCur < insertCur; searchCur++)
                    {
                        if (m_assigned[searchCur].Equals(m_assigned[insertCur]))
                        {
                            // 검색중 중복 배정임이 확인되면 다시 하도록 한다.
                            m_assigned[insertCur] = null;
                            insertCur--;
                            break;
                        }
                    }
                }

                // 최대 좌석번호 다음것(전체 좌석수)로 배정 좌석번호 지정
                m_curStep = normalSeats;

            }
            else
            {
                // TODO 앞자리부터 순서대로 채우지 않기, 미구현
            }

            showNames();/**/
        }

        private void showNames()
        {
            if (m_assigned != null)
            {
                // 자리 레이블을 배정받은 이름으로 변경
                for (int i = 0; i < m_seats.Count; i++)
                {
                    if (m_assigned[i] != null)
                    {
                        // 있는 경우 채워 넣는다
                        m_seats[i].Text = m_assigned[i];
                    }
                    else
                    {
                        // null 자리는 빈 자리가 된다
                        m_seats[i].Text = "　　　";
                    }
                }
            }
            else
            {
                //m_assigned이 생성되지 않았으면
                //초기화
                resetTimer();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            addToList(listPriority);
            /*
            // 이름 선택 되어 있을 때만.
            if (listName.SelectedIndex >= 0)
            {
                // 이름 칸의 텍스트를 이름 목록에 있으면 우선 순위 목록으로 옮깁니다
                listPriority.Items.Add(listName.Items[listName.SelectedIndex]);
                listName.Items.RemoveAt(listName.SelectedIndex);
            }//*/
        }

        /* 이름을 입력방식에서 선택하는 식으로 바꿨으므로 필요가 없다.
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // 이름 칸이 비어있지 않으면 추가합니다.
            if (!txtPriorityAdd.Text.Equals(""))
            {
                if (listName.Items.Contains(txtPriorityAdd.Text))
                {
                    // 이름 칸의 텍스트를 이름 목록에 있으면 우선 순위 목록으로 옮깁니다
                    listPriority.Items.Add(txtPriorityAdd.Text);
                    listName.Items.Remove(txtPriorityAdd.Text);
                    txtPriorityAdd.Text = "";
                    lblPriority.Text = "앞자리에 우선 배정할\n사람을 입력하세요";
                }
                else
                {
                    // 가지지 않은 경우 경고 표시
                    lblPriority.Text = "\"" + txtPriorityAdd.Text + "\"없음.\n다른 사람을 지정해주세요.";
                }
            }
            else
            {
                // 빈 칸이라면 일반 메시지 출력
                lblPriority.Text = "앞자리에 우선 배정할\n 사람을 입력하세요";
            }
        }//*/

        private void btnRemove_Click(object sender, EventArgs e)
        {
            removeFromList(listPriority);
            /*if (listPriority.SelectedIndex >= 0)
            {
                // 선택된 항목이 있는 경우

                // 항목을 이름 목록에 복귀시킵니다.
                listName.Items.Add((string)listPriority.Items[listPriority.SelectedIndex]);
                // 해당 항목을 우선순위에서 제외합니다.
                listPriority.Items.RemoveAt(listPriority.SelectedIndex);
                // 이전 항목을 선택
                if (listPriority.SelectedIndex > 0)
                {
                    listPriority.SelectedIndex--;
                }
            }//*/
        }


        private void btnAddLast_Click(object sender, EventArgs e)
        {
            addToList(listLast);
            
            /*
            // 이름 선택 되어 있을 때만.
            if (listName.SelectedIndex >= 0)
            {
                // 이름 칸의 텍스트를 이름 목록에 있으면 우선 순위 목록으로 옮깁니다
                listLast.Items.Add(listName.Items[listName.SelectedIndex]);
                listName.Items.RemoveAt(listName.SelectedIndex);
            }//*/
        }
        /* 입력 방식에서 선택 방식으로 변경해서 쓸모 없어짐
        private void btnAddLast_Click(object sender, EventArgs e)
        {
            // 이름 칸이 비어있지 않으면 추가합니다.
            if (!txtPriorityAdd.Text.Equals(""))
            {
                if (listName.Items.Contains(txtPriorityAdd.Text))
                {
                    // 이름 칸의 텍스트를 이름 목록에 있으면 우선 순위 목록으로 옮깁니다
                    listLast.Items.Add(txtPriorityAdd.Text);
                    listName.Items.Remove(txtPriorityAdd.Text);
                    txtPriorityAdd.Text = "";
                    lblPriority.Text = "뒷자리에 우선 배정할\n사람을 입력하세요";
                }
                else
                {
                    // 가지지 않은 경우 경고 표시
                    lblPriority.Text = "\"" + txtPriorityAdd.Text + "\"없음.\n다른 사람을 지정해주세요.";
                }
            }
            else
            {
                // 빈 칸이라면 일반 메시지 출력
                lblPriority.Text = "뒷자리에 우선 배정할\n사람을 입력하세요";
            }
        }//*/

        private void btnRemoveLast_Click(object sender, EventArgs e)
        {
            removeFromList(listLast);
            /*
            if (listLast.SelectedIndex >= 0)
            {
                // 선택된 항목이 있는 경우

                // 항목을 이름 목록에 복귀시킵니다.
                listName.Items.Add((string)listLast.Items[listLast.SelectedIndex]);
                // 해당 항목을 우선순위에서 제외합니다.
                listLast.Items.RemoveAt(listLast.SelectedIndex);
                // 이전 항목을 선택
                if (listLast.SelectedIndex > 0)
                {
                    listLast.SelectedIndex--;
                }
            }//*/
        }

        private void addToList(ListBox listTarget)
        {
            int selIndex = listName.SelectedIndex;
            // 이름 선택 되어 있을 때만.
            if (listName.SelectedIndex >= 0)
            {
                // 이름 칸의 텍스트를 이름 목록에 있으면 우선 순위 목록으로 옮깁니다
                listTarget.Items.Add(listName.Items[selIndex]);
                listName.Items.RemoveAt(selIndex);


                // 목록 선택 이벤트 무시 시작
                isUserChange = false;

                // 이전 항목을 선택
                if (listName.Items.Count > selIndex)
                {
                    //선택된 항목 번호가 범위를 초과하지 않는 경우
                    listName.SelectedIndex = selIndex;
                }
                else
                {
                    // 범위가 초과하면 범위 초과 예외가 발생하므로 하나 줄여본다.
                    // 왜냐면 하나씩밖에 추가제거를 안 하니까.
                    listName.SelectedIndex = --selIndex;
                }

                isUserChange = true;
                // 목록 선택 이벤트 무시 끝
            }
        }

        private void removeFromList(ListBox listTarget)
        {
            int selIndex = listTarget.SelectedIndex;
            if (selIndex >= 0)
            {
                // 선택된 항목이 있는 경우

                // 항목을 이름 목록에 복귀시킵니다.
                listName.Items.Add((string)listTarget.Items[selIndex]);
                
                // 해당 항목을 해당 리스트박스에서 제외합니다.
                listTarget.Items.RemoveAt(selIndex);

                // 목록 선택 이벤트 무시 시작
                isUserChange = false;

                // 이전 항목을 선택
                if (listTarget.Items.Count > selIndex)
                {
                    //선택된 항목 번호가 범위를 초과하지 않는 경우
                    listTarget.SelectedIndex = selIndex;
                }
                else
                {
                    // 범위가 초과하면 범위 초과 예외가 발생하므로 하나 줄여본다.
                    // 왜냐면 하나씩밖에 추가제거를 안 하니까.
                    listTarget.SelectedIndex = --selIndex;
                }

                isUserChange = true;
                // 목록 선택 이벤트 무시 끝
            }
        }

        private void btnAssignStepbyStep_Click(object sender, EventArgs e)
        {
            // 임의 배정 등 일부 UI 비활성화
            ruleButtons(true);

            // 버튼을 잠그고 더블클릭 방지 타이머를 기다립니다.
            btnAssignStepbyStep.Enabled = false;
            timerPreventDblClk.Enabled = true;

            if (m_curStep < 0)
            {
                // 아직 첫번째 자리도 배정되기 전에
                // 단계별 배정 버튼 클릭 되었으므로 

                getCheckedSeats();

                // 우선순위 자리 채우기용 명부 제작
                m_priorityList = new List<string>();
                for (int i = 0; i < listPriority.Items.Count; i++)
                {
                    m_priorityList.Add((string)listPriority.Items[i]);
                }

                // 일반 자리 채우기용 명부 제작
                m_namesList = new List<string>();
                for (int i = 0; i < listName.Items.Count; i++)
                {
                    m_namesList.Add((string)listName.Items[i]);
                }

                // 후순위 자리 채우기용 명부 제작
                m_lastList = new List<string>();
                for (int i = 0; i < listLast.Items.Count; i++)
                {
                    m_lastList.Add((string)listLast.Items[i]);
                }

                //명부를 섞습니다.
                m_priorityList = randomizeList(m_priorityList);
                m_namesList = randomizeList(m_namesList);
                m_lastList = randomizeList(m_lastList);

                // 실제 좌석 수만큼 m_assigned 배열 생성
                m_assigned = new string[m_seats.Count];

                //우선순위 배정 좌석수
                m_prioritySeats = m_priorityList.Count;
                if (m_prioritySeats > m_seats.Count)
                {
                    // 좌석수보다 우선순위 배정 수가 많은 경우
                    m_prioritySeats = m_seats.Count;
                }

                // 일반 배정 좌석번호의 최대는 우선순위 목록 수 ~ 우선순위 목록 수 + 이름 목록 수
                m_normalSeats = m_prioritySeats + m_namesList.Count;

                if (m_normalSeats > m_seats.Count)
                {
                    // 좌석수보다 사람 수가 많은 경우
                    m_normalSeats = m_seats.Count;
                }

                // 후순위 배정 좌석번호의 최대는 일반 배정 좌석수 ~ 일반배정 좌석수 + 후순위 목록 수
                m_lastSeats = m_normalSeats + m_lastList.Count ;

                if (m_lastSeats > m_seats.Count)
                {
                    // 좌석수보다 사람 수가 많은 경우
                    m_lastSeats = m_seats.Count;
                }

                // 첫번째 (0번) 자리를 배정하도록 커서를 이동합니다.
                m_curStep = 0;

            }
            else
            {
                // 할당 한 번 명령.
                // 할당을 명령하면 다음 타이머 틱 때 해당 번의 칸에 이름이 배정되도록 합니다.
                m_fireAssign = true;
            }
           

            // 타이머 시작
            timerRand.Enabled = true;

        }

        // 명부를 임의로 섞습니다. 배정때마다 기존 명부에서 삭제되고
        // 대신 임의로 섞인 새 명부를 반환합니다.
        private List<string> randomizeList(List<string> destructableList)
        {
            List<string> newList = new List<string>();

            int initialListCount = destructableList.Count;
            for(int i = 0; i < initialListCount; i++)
            {
                int cursor = m_rng.Next(0, destructableList.Count);
                newList.Add(destructableList[cursor]);
                destructableList.RemoveAt(cursor);
            }

            return newList;
        }

        private string getCheckedSeats()
        {
            // 전체 좌석 중 공석을 제외한 자리를 할당
            m_seats = new List<CheckBox>();

            //좌석 정보 문자열을 조립할 스트링 빌더
            StringBuilder sbSeats = new StringBuilder();
            int seatsPerRow = 0; // 한 줄당 좌석 수
            sbSeats.Append(FSA_TAG+"\r\n");

            for (int seatNo = 0; seatNo < m_totalSeats.Length; seatNo++)
            {
                //체크 되어있는거만 배정할 좌석 목록에 추가합니다.
                if (m_totalSeats[seatNo].Checked)
                {
                    m_seats.Add(m_totalSeats[seatNo]);
                    sbSeats.Append("1");
                }
                else
                {
                    // 아닌 경우 Text를 빈 칸으로 표시
                    m_totalSeats[seatNo].Text = "　　　";
                    sbSeats.Append("0");
                }

                seatsPerRow++;
                if (seatsPerRow >= 10)
                {
                    //한 줄당 좌석수 10번째인 경우 좌석정보에 줄바꿈 문자
                    sbSeats.Append("\r\n");
                    // 초기화
                    seatsPerRow = 0;
                }
            }

            setLabelAvailability(m_seats.Count);
            return sbSeats.ToString();
        }

        private void btnStopAssign_Click(object sender, EventArgs e)
        {
            // 배정값 초기화 버튼 클릭

            if (Environment.TickCount - tickClicked > 1000)
            {
                // 클릭된지 1초가 지났으면
                tickClicked = Environment.TickCount;
            }
            else
            {
                // 클릭한지 1초 안이면
                // 처음으로 초기화
                m_curStep = CUR_STEP_INIT_VAL;
                resetTimer();

                if (m_assigned != null)
                {
                    for (int cur = 0; cur < m_seats.Count; cur++)
                    {
                        m_assigned[cur] = null;
                    }
                }
                showNames();
            }
        }

        private void ruleButtons(bool isStepByStep)
        {
            btnAssignStepbyStep.Enabled = true;
            if (isStepByStep)
            {
                btnAssign.Enabled = false;
                btnAddPriority.Enabled = false;
                btnRemovePriority.Enabled = false;
                btnAddLast.Enabled = false;
                btnRemoveLast.Enabled = false;
                btnInsertName.Enabled = false;
                btnRemoveName.Enabled = false;

                for (int seatNo = 0; seatNo < m_totalSeats.Length; seatNo++)
                {
                    // 배정 중엔 좌석 가용 여부를 고칠 수 없게 합니다.
                    m_totalSeats[seatNo].AutoCheck = false;
                }
            }
            else
            {
                btnAssign.Enabled = true;
                btnAddPriority.Enabled = true;
                btnRemovePriority.Enabled = true;
                btnAddLast.Enabled = true;
                btnRemoveLast.Enabled = true;
                btnInsertName.Enabled = true;
                btnRemoveName.Enabled = true;

                for (int seatNo = 0; seatNo < m_totalSeats.Length; seatNo++)
                {
                    // 배정이 끝난 경우 좌석 가용 여부를 고칠 수 있게 합니다.
                    m_totalSeats[seatNo].AutoCheck = true;
                }
            }
        }

        private void timerRand_Tick(object sender, EventArgs e)
        {
            if (m_continuous) {
                //연속 배정 모드가 활성화되어있으면 자동으로 배정 버튼이 눌린 것으로 전환
                m_fireAssign = true;
            }
            if (m_curStep < m_prioritySeats)
            {
                // 현재 좌석 번호가 실제 배정받을 우선순위 명부 수 미만

                // 일반 명부 소스와 같으므로 그쪽 주석을 참고해주세요.
                // 우선순위 명부가 비어있으면 이름을 배정하지 않습니다.
                assignRandom(m_priorityList);

                /*
                bool hasLoop = m_priorityList.Count > 0;
                while (hasLoop)
                {
                    int cursor = m_rng.Next(0, m_priorityList.Count);
                    string currentGot = (string)m_priorityList[cursor];
                    m_assigned[m_curStep] = currentGot;

                    hasLoop = false;

                    if (m_fireAssign)
                    {
                        m_priorityList.RemoveAt(cursor);
                    }

                    /* 2018-06-26 23:30 이미 할당이 될 때부터 기할당된 이름은
                     // 명부에서 제외되서 중복체크가 필요없다.
                    for (int searchCur = 0; searchCur < m_curStep; searchCur++)
                    {
                        if (m_assigned[searchCur].Equals(currentGot))
                        {
                            m_assigned[m_curStep] = null;
                            if (m_fireAssign)
                            {
                                m_priorityList.Insert(cursor, currentGot);
                            }
                            hasLoop = true;
                            break;
                        }
                    }//*


                    if (m_fireAssign)
                    {
                        m_curStep++;
                        m_fireAssign = false;
                    }
                }//*/
            }
            else if (m_curStep < m_normalSeats)
            {
                // 현재 좌석 번호가 실제 배정받을 일반 명부 수 미만

                // 이름을 골라야 하므로 일반 명부가 채워져있으면
                // 고르는 작업을 시작합니다.
                assignRandom(m_namesList);
                /*bool hasLoop = m_namesList.Count > 0;
                while (hasLoop)
                {
                    // 일반 명부 중 임의의 수로 배정
                    int cursor = m_rng.Next(0, m_namesList.Count);
                    // 일반 명부에서 해당 번지의 이름 가져오기
                    string currentGot = (string)m_namesList[cursor];
                    // 배정 결과표에 이름 입력
                    m_assigned[m_curStep] = currentGot;

                    // 이름을 골랐으므로 더 이상 고르지 않도록 false로. 
                    hasLoop = false;
                    
                    if (m_fireAssign)
                    {
                        // 할당이 명령된 경우 해당 번의 우선순위 이름을 명부에서 삭제합니다.
                        m_namesList.RemoveAt(cursor);
                    }
                    
                    /* 2018-06-26 23:30 이미 할당이 될 때부터 기할당된 이름은
                     // 명부에서 제외되서 중복체크가 필요없다.
                    for (int searchCur = 0; searchCur < m_curStep; searchCur++)
                    {
                        if (m_assigned[searchCur].Equals(currentGot))
                        {
                            // 검색중 중복 배정임이 확인되면 다시 하도록 한다.
                            m_assigned[m_curStep] = null;

                            if (m_fireAssign)
                            {
                                // 할당이 명령된 경우 중복이 발생한 이름은 다시 명부에 넣어줍니다.
                                m_namesList.Insert(cursor, currentGot);
                            }

                            // 다시 이름을 골라야 하므로 true로 전환.
                            hasLoop = true;
                            break;
                        }
                    } //*

                    if (m_fireAssign)
                    {
                        // 중복을 피해 할당에 성공하였군요. 다음 좌석으로 이동
                        m_curStep++;

                        // 할당 명령을 완료하였습니다.
                        m_fireAssign = false;
                    }
                }//*/
            }
            else if (m_curStep < m_lastSeats)
            {
                // 현재 좌석 번호가 실제 배정받을 일반 명부 수 미만

                // 이름을 골라야 하므로 일반 명부가 채워져있으면
                // 고르는 작업을 시작합니다.
                assignRandom(m_lastList);
                /*bool hasLoop = m_lastList.Count > 0;
                while (hasLoop)
                {
                    /*
                    // 일반 명부 중 임의의 수로 배정
                    int cursor = m_rng.Next(0, m_lastList.Count);
                    // 일반 명부에서 해당 번지의 이름 가져오기
                    string currentGot = (string)m_lastList[cursor];
                    // 배정 결과표에 이름 입력
                    m_assigned[m_curStep] = currentGot;

                    // 이름을 골랐으므로 더 이상 고르지 않도록 false로. 
                    hasLoop = false;

                    if (m_fireAssign)
                    {
                        // 할당이 명령된 경우 해당 번의 우선순위 이름을 명부에서 삭제합니다.
                        m_lastList.RemoveAt(cursor);
                    }

                    /* 2018-06-26 23:30 이미 할당이 될 때부터 기할당된 이름은
                     // 명부에서 제외되서 중복체크가 필요없다.
                    for (int searchCur = 0; searchCur < m_curStep; searchCur++)
                    {
                        if (m_assigned[searchCur].Equals(currentGot))
                        {
                            // 검색중 중복 배정임이 확인되면 다시 하도록 한다.
                            m_assigned[m_curStep] = null;

                            if (m_fireAssign)
                            {
                                // 할당이 명령된 경우 중복이 발생한 이름은 다시 명부에 넣어줍니다.
                                m_lastList.Insert(cursor, currentGot);
                            }

                            // 다시 이름을 골라야 하므로 true로 전환.
                            hasLoop = true;
                            break;
                        }
                    }//*

                    if (m_fireAssign)
                    {
                        // 중복을 피해 할당에 성공하였군요. 다음 좌석으로 이동
                        m_curStep++;

                        // 할당 명령을 완료하였습니다.
                        m_fireAssign = false;
                    }
                }//*/
            }
            else
            {
                // 현재 좌석 번호가 실제 배정받을 전체 명부 수를 초과
                // 초기로 돌아감
                resetTimer();
            }
            showNames();
        }

        private bool assignRandom(List<string> listNames)
        {
            bool hasLoop = listNames.Count > 0;
            while (hasLoop)
            {
                // 명부 중 임의의 수로 배정
                int cursor = m_rng.Next(0, listNames.Count);
                // 명부에서 해당 번지의 이름 입력
                m_assigned[m_curStep] = listNames[cursor];

                // 이름을 골랐으므로 더 이상 고르지 않도록 false로. 
                hasLoop = false;

                if (m_fireAssign)
                {
                    // 할당이 명령된 경우 해당 번의 이름을 명부에서 삭제합니다.
                    listNames.RemoveAt(cursor);

                    // 중복을 피해 할당에 성공하였군요. 다음 좌석으로 이동
                    m_curStep++;

                    // 할당 명령을 완료하였습니다.
                    m_fireAssign = false;
                }
            }
            return hasLoop;
        }

        private void resetTimer()
        {
            // 임의 배정 버튼 활성화
            ruleButtons(false);

            //타이머 중단
            timerRand.Enabled = false;

            //연속배정 모드도 초기화
            m_continuous = false;
        }

        private void timerPreventDblClk_Tick(object sender, EventArgs e)
        {
            // 더블클릭 방지 타이머가 주기가 돌아오면
            timerPreventDblClk.Enabled = false;

            //버튼도 활성화.
            btnAssignStepbyStep.Enabled = true;

        }

        private void btnInsertName_Click(object sender, EventArgs e)
        {
            // 이름 추가 버튼을 누르면
            // 앞뒤 빈칸과 중복을 확인해 목록에 삽입하고 입력칸을 비웁니다.

            string nameToPut = txtName.Text.Trim();
            if (nameToPut.Length <= 0)
            {
                // 이름이 앞뒤 빈칸 빼니 없군요. 진행불가.
                return;
            }

            listName.Items.Add(nameToPut);

            // 이름을 저장하도록 합니다.
            m_namesRead = true;

            setLabelAvailability(m_cachedTotalSeats);

            txtName.Text = "";
        }

        private void btnRemoveName_Click(object sender, EventArgs e)
        {
            // 이름 제거 버튼을 누르면
            // 목록에서 선택된 이름을 입력칸으로 옮기고 목록에서 삭제합니다.

            //목록이 3개 있으므로 이 선택을 다 구분해야한다.
            ListBox selectedList = null;

            if (listPriority.SelectedIndex >= 0)
            {
                selectedList = listPriority;
            }

            if (listLast.SelectedIndex >= 0)
            {
                selectedList = listLast;
            }

            if(listName.SelectedIndex >= 0)
            {
                selectedList = listName;
            }

            if (selectedList != null)
            {
                // 선택된 목록이 있는 경우
                txtName.Text = removeNameFromList(selectedList);
            }
        }

        private string removeNameFromList(ListBox listTarget)
        {
            //이름을 추출한 다음 목록에서 삭제

            string nameFromList = "";
            int selIndex = listTarget.SelectedIndex;
            if (selIndex >= 0)
            {
                // 선택된 항목이 있는 경우

                //해당 항목을 목록에서 가져옵니다.
                nameFromList = (string)listTarget.Items[selIndex];

                // 해당 항목을 해당 리스트박스에서 제외합니다.
                listTarget.Items.RemoveAt(selIndex);


                // 목록 선택 이벤트 무시 시작
                isUserChange = false;


                // 이전 항목을 선택
                if (listTarget.Items.Count > selIndex)
                {
                    //선택된 항목 번호가 범위를 초과하지 않는 경우
                    listTarget.SelectedIndex = selIndex;
                }
                else
                {
                    // 범위가 초과하면 범위 초과 예외가 발생하므로 하나 줄여본다.
                    // 왜냐면 하나씩밖에 추가제거를 안 하니까.
                    listTarget.SelectedIndex = --selIndex;
                }


                isUserChange = true;
                // 목록 선택 이벤트 무시 끝
            }

            setLabelAvailability(m_cachedTotalSeats);

            return nameFromList;
        }


        
        
        private void deselectAllList()
        {
            // 모든 목록의 선택을 해제하기
            listPriority.SelectedIndex = -1;
            listLast.SelectedIndex = -1;
            listName.SelectedIndex = -1;
        }

        private void listName_SelectedIndexChanged(object sender, EventArgs e)
        {
            setFocus(listName);
        }

        private void listPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            setFocus(listPriority);
        }

        private void listLast_SelectedIndexChanged(object sender, EventArgs e)
        {
            setFocus(listLast);
        }

        private void setFocus(ListBox listTarget)
        {

            if (isUserChange)
            {
                int selectedIndexBackup = listTarget.SelectedIndex;
                // 목록 선택 이벤트 무시 시작

                isUserChange = false;

                //모든 목록을 선택 해제
                deselectAllList();

                //다시 선택 복구
                listTarget.SelectedIndex = selectedIndexBackup;

                isUserChange = true;
                // 이벤트 처리 끝
            }
        }
    }
}
